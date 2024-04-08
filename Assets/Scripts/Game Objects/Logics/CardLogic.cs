using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogic : MonoBehaviour
{
    public Game_Manager gameManager;

    public EnumManager enumConverter;

    public AudioManager audioManager;

    public UIManager U_I;

    public Type type;
    public List<PlayType> playTypes;
    public List<Trait> traits;

    public PlayerManager cardOwner, cardController;
    public Transform cardFace, cardBack, cardImage, cardOutline;
    public Sprite image;

    public string id, cardName, cardText, flavorText;

    public bool isFaceDown, isNormalColour;

    public Location currentLocation;
    public int locationOrderNumber;

    public List<CardLogic> targets = new();
    public List<CardLogic> validTargets = new();
    public List<Effect> effects = new();

    public int effectCountNumber;
    public int subCountNumber;
    public Effect focusEffect;
    public SubEffect focusSubEffect;

    public float movementSpeed = 3f;

    private IEnumerator ActivationCoroutine(int effectNumber, int subEffectNumber)
    {
        gameManager.isActivatingEffect = true;
        gameManager.DisableRayBlocker();

        //these technically don't activate, they are passives
        if (effects[effectNumber].SubEffects[subEffectNumber].effectType == EffectTypes.WhileDeployed)
        {
            EffectActivationAfterAnimation(effectNumber, subEffectNumber);
            yield break;
        }

        gameManager.StateChange(GameState.EffectActivation);
        audioManager.NewAudioPrefab(audioManager.effectActivation);
        float distance = Vector3.Distance(transform.position, cardController.activationZone.position);
        Vector3 originalPosition = transform.position;
        Vector3 direction = (cardController.activationZone.position - transform.position).normalized;
        float distanceTravelled = 0;
        while (distanceTravelled < distance)
        {
            Vector3 translationDistance = (cardController.activationZone.position - transform.position);
            if (Vector3.SqrMagnitude(translationDistance) <= Vector3.SqrMagnitude(direction))
                transform.position = cardController.activationZone.position;
            else
                transform.Translate(direction * movementSpeed, Space.World);
            distanceTravelled = Vector3.Distance(originalPosition, transform.position);
            yield return null;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = new(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;

        EffectActivationAfterAnimation(effectNumber, subEffectNumber);
        yield break;
    }

    private IEnumerator ResolutionCoroutine(int effectNumber, int subEffectNumber)
    {
        gameManager.DisableRayBlocker();

        //these technically don't resolve
        if (effects[effectNumber].SubEffects[subEffectNumber].effectType == EffectTypes.WhileDeployed)
        {
            EffectResolutionAfterAnimation(effectNumber, subEffectNumber);
            yield break;
        }

        gameManager.StateChange(GameState.EffectResolution);
        float distance = Vector3.Distance(transform.position, cardController.activationZone.position);
        Vector3 originalPosition = transform.position;
        Vector3 direction = (cardController.activationZone.position - transform.position).normalized;
        float distanceTravelled = 0;
        while (distanceTravelled < distance)
        {
            Vector3 translationDistance = (cardController.activationZone.position - transform.position);
            if (Vector3.SqrMagnitude(translationDistance) <= Vector3.SqrMagnitude(direction))
                transform.position = cardController.activationZone.position;
            else
                transform.Translate(direction * movementSpeed, Space.World);
            distanceTravelled = Vector3.Distance(originalPosition, transform.position);
            yield return null;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = new(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        EffectResolutionAfterAnimation(effectNumber, subEffectNumber);
        yield break;
    }

    public List<CardLogic> GetValidTargets(int effectNumber, int subEffectNumber)
    {
        List<CardLogic> allTargetsList = new(FindObjectsOfType<CardLogic>());
        List<CardLogic> returnList = new();
        focusEffect = effects[effectNumber];
        focusSubEffect = focusEffect.SubEffects[subEffectNumber];
        if (focusSubEffect.targetLocations == null)
            return returnList;

        List<CardLogic> camoTargets = new();
        foreach (CardLogic logic in allTargetsList)
        {
            logic.TryGetComponent<CombatantLogic>(out var combatantLogic);
            if (logic.currentLocation == Location.Field)
                if (combatantLogic.targetState == TargetState.Camouflage)
                {
                    camoTargets.Add(logic);
                    allTargetsList.Remove(logic);
                }
        }
        if (allTargetsList.Count == 0)
            allTargetsList.AddRange(camoTargets);

        EffectsUsed effectUsed = focusSubEffect.effectUsed;
        Controller controller = focusSubEffect.effectTargetController;

        foreach (CardLogic target in allTargetsList)
        {
            target.TryGetComponent<CombatantLogic>(out var combatantStats);
            target.TryGetComponent<PlayableLogic>(out var playableStats);
            // basic targeting requirements... don't target wrong location, don't target wrong owner or wrong card types
            if (target == this && focusEffect.SubEffects[subEffectNumber].AllowSelfTarget == false)
                continue;
            if (focusSubEffect.targetLocations != null && !focusSubEffect.targetLocations.Contains(target.currentLocation))
                continue;
            if (target.cardController == cardController && controller == Controller.Opponent)
                continue;
            if (target.cardController != cardController && controller == Controller.Player)
                continue;

            if(focusSubEffect.effectTargetTypes!= null && !focusSubEffect.effectTargetTypes.Contains(target.type))
                continue;

            // don't target max hp with healing effects
            if (effectUsed == EffectsUsed.Regeneration && (combatantStats == null || combatantStats.maxHp == combatantStats.currentHp))
                continue;
            // don't target no hp with damage effects
            if (effectUsed == EffectsUsed.Damage && (combatantStats == null || combatantStats.currentHp <= 0))
                continue;
            //don't add targets with higher cost when paying for revive or deploy cost
            if ((effectUsed == EffectsUsed.Revive || effectUsed == EffectsUsed.Deploy) && cardController.costCount < playableStats.cost)
                continue;

            //don't target no timed debuffs with detonate
            if (effectUsed == EffectsUsed.Detonate && (combatantStats == null || (combatantStats.DebuffCheck(Debuffs.Bombed) == null &&
                combatantStats.DebuffCheck(Debuffs.Burned) == null && combatantStats.DebuffCheck(Debuffs.Poisoned) == null)))
                continue;
            //don't target no bombs with bomb detonate
            if (effectUsed == EffectsUsed.BombDetonate && (combatantStats == null || combatantStats.DebuffCheck(Debuffs.Bombed) == null))
                continue;
            //don't target no burns with burn detonate
            if (effectUsed == EffectsUsed.BurnDetonate && (combatantStats == null || combatantStats.DebuffCheck(Debuffs.Burned) == null))
                continue;
            //don't target no poisons with poison detonate
            if (effectUsed == EffectsUsed.PoisonDetonate && (combatantStats == null || combatantStats.DebuffCheck(Debuffs.Poisoned) == null))
                continue;

            if (focusSubEffect.TargetStats != null)
            {
                for (int i = 0; i < focusSubEffect.TargetStats.Count; i++)
                {
                    string checkedStat = focusSubEffect.TargetStats[i];
                    string condition = focusSubEffect.TargetStatConditions[i];
                    int amount = focusSubEffect.TargetStatAmounts[i];
                    int stat = 0;
                    //as more types of effects are added, more checks will be needed
                    if (checkedStat == "none")
                    {
                        returnList.Add(target);
                        continue;
                    }
                    if (checkedStat == "current atk")
                    {
                        if (combatantStats == null)
                            continue;
                        stat = combatantStats.currentAtk;
                    }

                    if (checkedStat == "cost")
                    {
                        if (playableStats == null)
                            continue;
                        stat = playableStats.cost;
                    }
                    if (checkedStat[..3] == "has")
                        if (!target.cardName.Contains(checkedStat[3..]))
                            continue;
                    if (checkedStat[..4] == "nhas")
                        if (target.cardName.Contains(checkedStat[4..]))
                            continue;
                    if (checkedStat[..2] == "is")
                        if (target.cardName != checkedStat[2..])
                            continue;
                    if (checkedStat[..3] == "not")
                        if (target.cardName == checkedStat[3..])
                            continue;

                    // don't target stats not equal requirements
                    if (condition == "==" && stat != amount)
                        continue;
                    if (condition == ">=" && stat < amount)
                        continue;
                    if (condition == "<=" && stat > amount)
                        continue;
                    if (condition == ">" && stat <= amount)
                        continue;
                    if (condition == "<" && stat >= amount)
                        continue;
                }
            }
            returnList.Add(target);
        }
        if (returnList.Count < 1)
            Debug.Log($"No valid targets for {cardName}'s {effectUsed} ability at {gameManager.turnPlayer.PlayerName}'s turn at phase {gameManager.currentPhase}");
        return returnList;
    }

    public void EffectActivation(int countNumber, int subCount) =>
        StartCoroutine(ActivationCoroutine(countNumber, subCount));

    private void EffectActivationAfterAnimation(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        focusSubEffect = focusEffect.SubEffects[subCount];
        switch (focusSubEffect.effectType)
        {
            //on play
            case EffectTypes.Deployment:
            //in response to
            case EffectTypes.Chain:
            //continuous on field
            case EffectTypes.WhileDeployed:
                TargetCheck(countNumber, subCount);
                break;
            //while on field, manual trigger
            case EffectTypes.Deployed:
            //while in gy manual trigger
            case EffectTypes.Vengeance:
                if (focusEffect.activationLocations.Contains(currentLocation))
                    TargetCheck(countNumber, subCount);
                break;
        }
        SetFocusCardLogic();
    }

    public void EffectResolution(int countNumber, int subCount) => StartCoroutine(ResolutionCoroutine(countNumber, subCount));

    private void EffectResolutionAfterAnimation(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        focusSubEffect = focusEffect.SubEffects[subCount];
        EffectsUsed effectUsed = focusSubEffect.effectUsed;
        int effectAmount = focusSubEffect.effectAmount;
        effectCountNumber = countNumber;
        subCountNumber = subCount;
        SetFocusCardLogic();
        EffectLogger(focusEffect,subCount,targets);
        List<CardLogic> tempTargets = new(targets);

        switch (effectUsed)
        {
            //for effects that need special targeting
            case EffectsUsed.Target:
                TargetEffectLogic(countNumber, subCount);
                break;

            //effects that access game manager methods, can be optimized further
            case EffectsUsed.Reinforce:
                StartCoroutine(gameManager.DrawCard(effectAmount, cardController));
                break;
            case EffectsUsed.BloodRecovery:
                gameManager.CostChange(effectAmount, cardController, true);
                break;
            case EffectsUsed.Recruit:
                foreach (CardLogic target in tempTargets)
                    StartCoroutine(gameManager.SearchCard(target, target.cardController));
                break;
            case EffectsUsed.Recover:
                foreach (CardLogic target in tempTargets)
                    StartCoroutine(gameManager.RecoverCard(target, cardController));
                break;

            //these are undefined effects
            case EffectsUsed.Damage:
            case EffectsUsed.Regeneration:
            case EffectsUsed.Shatter:
            case EffectsUsed.Detonate:
            case EffectsUsed.BurnDetonate:
            case EffectsUsed.BombDetonate:
            case EffectsUsed.PoisonDetonate:
            case EffectsUsed.Bounce:
                foreach (CardLogic target in tempTargets)
                    target.EffectHandler(focusEffect, subCount);
                break;

            //these are buffs
            case EffectsUsed.Rally:
            case EffectsUsed.Vigor:
            case EffectsUsed.Taunt:
            case EffectsUsed.Stealth:
            case EffectsUsed.Armor:
            case EffectsUsed.Camouflage:
            case EffectsUsed.Barrier:
                foreach (CardLogic target in tempTargets)
                    target.EffectHandler(focusEffect, subCount);
                break;

            //these are debuffs
            case EffectsUsed.Terrify:
            case EffectsUsed.Intimidate:
            case EffectsUsed.Weaken:
            case EffectsUsed.Sleep:
            case EffectsUsed.Stun:
            case EffectsUsed.Provoke:
            case EffectsUsed.Disarm:
            case EffectsUsed.Burn:
            case EffectsUsed.Poison:
            case EffectsUsed.Bomb:
            case EffectsUsed.Spot:
                foreach (CardLogic target in tempTargets)
                    target.EffectHandler(focusEffect, subCount);
                break;
                //these are unedefined effects
            case EffectsUsed.FreeRevive:
            case EffectsUsed.Revive:
            case EffectsUsed.FreeDeploy:
            case EffectsUsed.Deploy:
                foreach (CardLogic target in tempTargets)
                    target.GetComponent<PlayableLogic>().PlayCard(effectUsed, cardController);
                break;
            default:
                Debug.Log("Attempting to use an unimplemented effect");
                break;
        }
        if (!gameManager.isWaitingForResponse)
            FinishResolution(countNumber, subCount);
    }

    public void FinishResolution(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        gameManager.isActivatingEffect = false;
        if (focusEffect.currentActivations < focusEffect.maxActivations)
            focusEffect.currentActivations++;

        //chainlist for effect triggers
        gameManager.GetEffectTriggers(countNumber, subCount, this);

        CheckSubsequentEffects(countNumber, subCount);
    }

    private void CheckSubsequentEffects(int countNumber, int subCount)
    {
        if (ResolveSubsequentSubeffects(countNumber, subCount))
        {
            gameManager.ClearEffectTargetImages();
            targets?.Clear();
            validTargets?.Clear();
            if (type == Type.Spell)
                gameObject.GetComponent<PlayableLogic>().MoveToGrave();
            audioManager.NewAudioPrefab(audioManager.effectResolution);
            //resolve chain after all possible effect chains are linked
            if (cardController.isAI)
                cardController.AIManager.isPerformingAction = false;
            if (!gameManager.isActivatingEffect && !gameManager.isPlayingCard)
                gameManager.ChainResolution();
        };
    }

    private bool ResolveSubsequentSubeffects(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        focusSubEffect = focusEffect.SubEffects[subCount];
        
        //resolve subsequent subeffects in the same effect if there is any
        if (focusEffect.SubEffects.Count <= subCount + 1)
            return true;
        SubEffect nextSubEffect = focusEffect.SubEffects[subCount + 1];
        if (focusSubEffect.effectType != nextSubEffect.effectType)
            return true;
        if (nextSubEffect.EffectActivationIsMandatory == false)
        {
            effectCountNumber = countNumber;
            subCountNumber = subCount + 1;
            SetFocusCardLogic();
            if (cardController.isAI)
                OptionalEffectResolution(cardController.AIManager.ActivateOptionalEffect());
            else
                gameManager.EnableActivationPanel();
            return false;
        }
        //dependent on targets of previous effect
        if (nextSubEffect.EffectTargetAmount == 98)
            EffectResolution(countNumber, subCount + 1);
        else
        {
            targets?.Clear();
            validTargets?.Clear();
            TargetCheck(countNumber, subCount + 1);
        }
        return false;
    }

    //some effects modify effect amount based on count of something multiplied by a modifier, this handles it
    private void TargetEffectLogic(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        focusSubEffect = focusEffect.SubEffects[subCount];
        List<int> effectAmountIndexesToChange = new();
        foreach(SubEffect subEffect in focusEffect.SubEffects)
            if (subEffect.effectAmount == 98)
                effectAmountIndexesToChange.Add(focusEffect.SubEffects.FindIndex(a=>a==subEffect));
        targets[0].TryGetComponent<CombatantLogic>(out var combatantStats);
        targets[0].TryGetComponent<PlayableLogic>(out var playableStats);

        foreach (int index in effectAmountIndexesToChange)
        {
            if (focusSubEffect.TargetCountModifier != 0)
            {
                focusSubEffect.effectAmount = Mathf.CeilToInt(targets.Count * focusSubEffect.TargetCountModifier);
                continue;
            }

            string checkedStat = focusSubEffect.TargetStats[0];
            switch (checkedStat)
            {
                case "current atk":
                    focusSubEffect.effectAmount = combatantStats.currentAtk;
                    break;
                case "cost":
                    focusSubEffect.effectAmount = playableStats.cost;
                    break;
                default:
                    Debug.Log("unimplemented target stat");
                    break;
            }
        }
    }

    private void TargetCheck(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        focusSubEffect = focusEffect.SubEffects[subCount];
        if (focusSubEffect.EffectTargetAmount == 0)
        {
            EffectResolution(countNumber, subCount);
            return;
        }

        if (targets == null || targets.Count < focusSubEffect.EffectTargetAmount)
        {
            gameManager.StateChange(GameState.Targeting);
            validTargets = new(GetValidTargets(countNumber, subCount));
            if (focusSubEffect.targetingType == TargetingTypes.Manual)
            {
                //if no valid targets, end effect
                if (validTargets.Count == 0)
                {
                    EffectResolution(countNumber, subCount);
                    return;
                }
                effectCountNumber = countNumber;
                subCountNumber = subCount;
                SetFocusCardLogic();
                foreach (CardLogic target in validTargets)
                {
                    if (target.type == Type.Fighter && target.currentLocation == Location.Field)
                        target.cardController.effectTargets[target.locationOrderNumber].SetActive(true);
                    if (target.type == Type.God && target.currentLocation == Location.Field)
                        target.cardController.heroEffectTarget.SetActive(true);
                    if (target.currentLocation == Location.Grave)
                        target.cardController.graveTarget.SetActive(true);
                    if (target.currentLocation == Location.Deck)
                        target.cardController.deckTarget.SetActive(true);
                }
                if (cardController.isAI)
                {
                    cardController.AIManager.GetEffectTarget();
                    return;
                }
                if (focusSubEffect.targetLocations.Contains(Location.Field))
                    gameManager.EnableCardScrollScreen(validTargets, !focusSubEffect.EffectActivationIsMandatory);
                return;
            }
            if (focusSubEffect.targetingType == TargetingTypes.Auto)
            {
                AutoTargetAcquisition(countNumber, subCount);
                return;
            }
            if (focusSubEffect.targetingType == TargetingTypes.Random)
            {
                RandomTargetAcquisition(countNumber, subCount);
                return;
            }
            if (focusSubEffect.targetingType == TargetingTypes.Trigger)
            {
                targets = new();
                return;
            }
        }
        if (gameManager.gameState != GameState.Targeting && targets.Count == 0)
        {
            EffectResolution(countNumber, subCount);
            return;
        }
    }

    //called by ux manager clicked GameObject method with current focus card logic count and subcount
    public void ManualTargetAcquisition(int countNumber, int subCount)
    {
        if (gameManager.gameState != GameState.Targeting)
            return;

        CardLogic targeter = gameManager.currentFocusCardLogic;
        if (!targeter.validTargets.Contains(this))
            return;
        if (targeter.targets == null)
            targeter.targets = new();
        else if (targeter.targets.Contains(this))
            return;
        targeter.targets.Add(this);
        //if you hit the needed amount of targets or all valid targets are taken, resolve
        if (targeter.targets.Count == targeter.effects[countNumber].SubEffects[subCount].EffectTargetAmount || 
            targeter.targets.Count == targeter.validTargets.Count)
        {
            gameManager.DisableCardScrollScreen();
            targeter.EffectResolution(countNumber, subCount);
            return;
        }
    }

    public void ManualTargetRemoval()
    {
        if (gameManager.gameState != GameState.Targeting)
            return;
        CardLogic targeter = gameManager.currentFocusCardLogic;
        if (targeter.targets == null)
            return;
        if (targeter.targets.Contains(this))
            targeter.targets.Remove(this);
    }

    //less auto more all target
    public void AutoTargetAcquisition(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        focusSubEffect = focusEffect.SubEffects[subCount];
        //auto self target effects
        if (focusSubEffect.AllowSelfTarget == true && focusSubEffect.EffectTargetAmount == 1)
            targets = new() { this };
        else
            targets = new(validTargets);
        EffectResolution(countNumber, subCount);
        return;
    }

    public void RandomTargetAcquisition(int countNumber, int subCount)
    {
        int targetsLeft = effects[countNumber].SubEffects[subCount].EffectTargetAmount;
        while (targetsLeft > 0 && validTargets.Count > targets.Count)
        {
            int randomNumber = Random.Range(0, validTargets.Count);
            if (targets.Contains(validTargets[randomNumber]))
                continue;
            targets.Add(validTargets[randomNumber]);
            targetsLeft--;
        }
        EffectResolution(countNumber, subCount);
        return;
    }

    public void OptionalEffectResolution(bool used)
    {
        if (!used)
        {
            CheckSubsequentEffects(effectCountNumber, subCountNumber);
            return;
        }
        //if you need the targets from previous effect to resolve
        if (effects[effectCountNumber].SubEffects[subCountNumber].TargetingType != TargetingTypes.Undefined && 
            effects[effectCountNumber].SubEffects[subCountNumber].EffectTargetAmount == 98)
            EffectResolution(effectCountNumber, subCountNumber);
        else
        {
            targets.Clear();
            validTargets.Clear();
            EffectActivation(effectCountNumber, subCountNumber);
        }
        //otherwise activate effect afresh
    }

    //reset activation count for effects usually at turn start
    public void EffectRefresh()
    {
        foreach (Effect effect in effects)
            effect.currentActivations = 0;
    }

    public void SetFocusCardLogic()
    {
        if (gameManager.currentFocusCardLogic != null)
            gameManager.currentFocusCardLogic.RemoveFocusCardLogic();
        gameManager.currentFocusCardLogic = this;
        EnableCardOutline();
    }

    public void RemoveFocusCardLogic()
    {
        gameManager.currentFocusCardLogic = null;
        DisableCardOutline();
    }

    public void FlipFaceUp()
    {
        isFaceDown = false;
        cardBack.gameObject.SetActive(false);
        cardFace.gameObject.SetActive(true);
        cardImage.gameObject.SetActive(true);
        audioManager.NewAudioPrefab(audioManager.flipCard);
    }

    public void FlipFaceDown()
    {
        isFaceDown = true;
        cardBack.gameObject.SetActive(true);
        cardFace.gameObject.SetActive(false);
        cardImage.gameObject.SetActive(false);
        audioManager.NewAudioPrefab(audioManager.flipCard);
    }

    public void EnableCardOutline()
    {
        if (currentLocation != Location.Grave)
            cardOutline.gameObject.SetActive(true);
        else
            cardOwner.underworldManager.outline.SetActive(true);
    }

    public void DisableCardOutline()
    {
        if (currentLocation != Location.Grave)
            cardOutline.gameObject.SetActive(false);
        else
            cardOwner.underworldManager.outline.SetActive(false);
    }
    public void LocationChange(Location location, int num)
    {
        LocationHistoryEntry locationLog = new(location)
        {
            logIndex = gameManager.gameLogHistoryEntries.Count,
            loggedCard = this,
            loggedLocation = currentLocation
        };
        gameManager.gameLogHistoryEntries.Add(locationLog);
        currentLocation = location;
        locationOrderNumber = num;
    }

    public void GreyScaleEffect()
    {
        isNormalColour = false;
        cardFace.GetComponent<SpriteRenderer>().color = Color.grey;
        cardImage.GetComponent<SpriteRenderer>().color = Color.grey;
    }

    public void NormalColour()
    {
        isNormalColour = true;
        cardFace.GetComponent<SpriteRenderer>().color = Color.white;
        cardImage.GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void ControllerSwap(PlayerManager player)
    {
        cardController = player;
        gameObject.transform.rotation = player.deck.transform.rotation;
    }

    virtual public void StatAdjustment(int value, Status status) => Debug.Log($"Failed virtual override for status of {cardName}");

    public void EffectLogger(Effect effect, int index, List<CardLogic> cards)
    {

        EffectLogHistoryEntry effectLogHistoryEntry = new(effect, effect.SubEffects[index].effectUsed, cards)
        {
            logIndex = gameManager.gameLogHistoryEntries.Count,
            loggedCard = this,
            loggedLocation = currentLocation
        };
        gameManager.gameLogHistoryEntries.Add(effectLogHistoryEntry);
    }

    public void EffectHandler(Effect effect, int effectIndex)
    {
        //apart from where specified, duration set to 0(not defined) indicates an infinite duration, be careful with blanks
        CombatantLogic combatantLogic = GetComponent<CombatantLogic>();
        MonsterLogic monsterLogic = GetComponent<MonsterLogic>();
        CardLogic logic = gameManager.currentFocusCardLogic;
        int effectAmount = effect.SubEffects[effectIndex].EffectAmount;
        EffectsUsed effectsUsed = effect.SubEffects[effectIndex].effectUsed;
        List<CardStatus> statuses = new();

        switch (effectsUsed)
        {
            case EffectsUsed.Rally:
                combatantLogic.StatAdjustment(effectAmount, Status.AtkGain);
                break;
            case EffectsUsed.Damage:
                combatantLogic.TakeDamage(effectAmount, false);
                break;
            case EffectsUsed.Regeneration:
                combatantLogic.Heal(effectAmount);
                break;
            case EffectsUsed.Vigor:
                combatantLogic.StatAdjustment(effectAmount, Status.AtkGain);
                combatantLogic.StatAdjustment(effectAmount, Status.HpGain);
                break;
            case EffectsUsed.Terrify:
                combatantLogic.StatAdjustment(effectAmount, Status.HpLoss);
                combatantLogic.StatAdjustment(effectAmount, Status.AtkLoss);
                break;
            case EffectsUsed.Intimidate:
                combatantLogic.StatAdjustment(effectAmount, Status.AtkLoss);
                break;
            case EffectsUsed.Weaken:
                combatantLogic.StatAdjustment(effectAmount, Status.HpLoss);
                break;
            case EffectsUsed.Shatter:
                monsterLogic.MonsterDeath();
                break;
            case EffectsUsed.Taunt:
                Taunt taunt = new(logic, this, effect.duration);
                combatantLogic.SetTargetStatus(taunt, TargetState.Taunt);
                break;
            case EffectsUsed.Stealth:
                Stealth stealth = new(logic, this, effect.duration);
                combatantLogic.SetTargetStatus(stealth, TargetState.Stealth);
                break;
                //everything above here works fine
            case EffectsUsed.Camouflage:
                Camouflage camouflage = new(logic, this, effect.duration);
                combatantLogic.SetTargetStatus(camouflage, TargetState.Stealth);
                break;
                //everything from here works fine
            case EffectsUsed.Armor:
                Armor armor = new(logic, this, effectAmount, effect.duration);
                combatantLogic.AddNonStackingBuff(armor);
                break;
            case EffectsUsed.Barrier:
                Barrier barrier = new(logic, this, effectAmount, effect.duration);
                combatantLogic.AddNonStackingBuff(barrier);
                break;
                //till here
            case EffectsUsed.Sleep:
                Sleep sleep = new(logic, this, effect.duration);
                combatantLogic.AddNonStackingDebuff(sleep);
                break;
            case EffectsUsed.Stun:
                Stun stun = new(logic, this, effect.duration);
                combatantLogic.AddNonStackingDebuff(stun);
                break;
            case EffectsUsed.Provoke:
                Provoke provoke = new(logic, this, effect.duration);
                combatantLogic.SetTargetStatus(provoke, TargetState.Stealth);
                break;
            case EffectsUsed.Disarm:
                Disarm disarm = new(logic, this, effect.duration);
                combatantLogic.AddNonStackingDebuff(disarm);
                break;
                //everything from here works fine
            case EffectsUsed.Burn:
                //burns have a default timer of two turns, if duration is set to 0/not defined(int), default applies
                Burn burn = new(logic, this, effect.duration);
                combatantLogic.cardStatuses.Add(burn);
                cardController.SetStatusIcon(locationOrderNumber, burn);
                break;
            case EffectsUsed.Poison:
                //poisons have a default timer of four turns, if duration is set to 0/not defined(int), default applies
                Poison poison = new(logic, this, effect.duration);
                combatantLogic.cardStatuses.Add(poison);
                cardController.SetStatusIcon(locationOrderNumber, poison);
                break;
            case EffectsUsed.Bomb:
                //bombs have a default timer of three turns, if duration is set to 0/not defined(int), default applies
                Bomb bomb = new(logic, this, effect.duration);
                combatantLogic.cardStatuses.Add(bomb);
                cardController.SetStatusIcon(locationOrderNumber, bomb);
                break;
                //till here
            case EffectsUsed.Spot:
                Spot spot = new(logic, this, effect.duration);
                combatantLogic.AddNonStackingDebuff(spot);
                break;
                //this works
            case EffectsUsed.Bounce:
                StartCoroutine(monsterLogic.BounceCard());
                break;
            case EffectsUsed.Detonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    statuses.Add(status);
                foreach (CardStatus status in statuses)
                    status.DetonateActions(gameManager);
                break;
            case EffectsUsed.BombDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Bomb)
                        statuses.Add(status);
                foreach (CardStatus status in statuses)
                    status.DetonateActions(gameManager);
                break;
            case EffectsUsed.BurnDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Burn)
                        statuses.Add(status);
                foreach (CardStatus status in statuses)
                    status.DetonateActions(gameManager);
                break;
            case EffectsUsed.PoisonDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Poison)
                        statuses.Add(status);
                foreach (CardStatus status in statuses)
                    status.DetonateActions(gameManager);
                break;
            case EffectsUsed.BuffDispel:
                for (int i = effectAmount; i > 0;)
                {
                    if (combatantLogic.cardStatuses.Count > 0)
                    {
                        CardStatus status = combatantLogic.BuffCheck(Buffs.Undefined);
                        if (status != null)
                            combatantLogic.RemoveCardStatus(status);
                    }
                    i--;
                } 
                break;
            case EffectsUsed.DebuffDispel:
                for (int i = effectAmount; i > 0;)
                {
                    if (combatantLogic.cardStatuses.Count > 0)
                    {
                        CardStatus status = combatantLogic.DebuffCheck(Debuffs.Undefined);
                        if (status != null)
                            combatantLogic.RemoveCardStatus(status);
                    }
                    i--;
                }
                break;
            case EffectsUsed.Silence:
                Silence silence = new(logic, this, effect.duration);
                combatantLogic.AddNonStackingDebuff(silence);
                break;
            default:
                Debug.Log("effect not found");
                return;
        }
    }
}