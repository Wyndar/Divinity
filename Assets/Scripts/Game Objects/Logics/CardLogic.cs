using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogic : MonoBehaviour
{
    public Game_Manager gameManager;

    public AudioManager audioManager;

    public UIManager U_I;

    public Type type;
    public Rarity rarity;
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

    public SubEffect focusSubEffect;

    public float movementSpeed = 3f;

    private IEnumerator ActivationCoroutine(SubEffect subEffect)
    {
        gameManager.isActivatingEffect = true;
        gameManager.DisableRayBlocker();

        //these technically don't activate, they are passives
        if (subEffect.effectType == EffectTypes.WhileDeployed)
        {
            EffectActivationAfterAnimation(subEffect);
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

        EffectActivationAfterAnimation(subEffect);
        yield break;
    }

    private IEnumerator ResolutionCoroutine(SubEffect subEffect)
    {
        gameManager.DisableRayBlocker();

        //these technically don't resolve
        if (subEffect.effectType == EffectTypes.WhileDeployed)
        {
            EffectResolutionAfterAnimation(subEffect);
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
        EffectResolutionAfterAnimation(subEffect);
        yield break;
    }

    public List<CardLogic> GetValidTargets(SubEffect subEffect)
    {
        List<CardLogic> allTargetsList = new(FindObjectsOfType<CardLogic>());
        List<CardLogic> returnList = new();
        if (subEffect.targetLocations == null)
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

        EffectsUsed effectUsed = subEffect.effectUsed;
        Controller controller = subEffect.effectTargetController;

        foreach (CardLogic target in allTargetsList)
        {
            target.TryGetComponent<CombatantLogic>(out var combatantStats);
            target.TryGetComponent<PlayableLogic>(out var playableStats);
            // basic targeting requirements... don't target wrong location, don't target wrong owner or wrong card types
            if (target == this && subEffect.AllowSelfTarget == false)
                continue;
            if (subEffect.targetLocations != null && !subEffect.targetLocations.Contains(target.currentLocation))
                continue;
            if (target.cardController == cardController && controller == Controller.Opponent)
                continue;
            if (target.cardController != cardController && controller == Controller.Player)
                continue;

            if(subEffect.effectTargetTypes!= null && !subEffect.effectTargetTypes.Contains(target.type))
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

            if (subEffect.TargetStats != null)
            {
                for (int i = 0; i < subEffect.TargetStats.Count; i++)
                {
                    string checkedStat = subEffect.TargetStats[i];
                    string condition = subEffect.TargetStatConditions[i];
                    int amount = subEffect.TargetStatAmounts[i];
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

    public void EffectActivation(SubEffect subEffect) =>
        StartCoroutine(ActivationCoroutine(subEffect));

    private void EffectActivationAfterAnimation(SubEffect subEffect)
    {
        focusSubEffect = subEffect;
        switch (subEffect.effectType)
        {
            //on play
            case EffectTypes.Deployment:
            //in response to
            case EffectTypes.Chain:
            //continuous on field
            case EffectTypes.WhileDeployed:
            //while on field, manual trigger
            case EffectTypes.Deployed:
            //on destroyed, auto trigger
            case EffectTypes.Vengeance:
            //on attacked, auto trigger
            case EffectTypes.Counter:
                if(subEffect.parentEffect.activationLocations==null || subEffect.parentEffect.activationLocations.Contains(currentLocation))
                TargetCheck(subEffect);
                break;
        }
        SetFocusCardLogic();
    }

    public void EffectResolution(SubEffect subEffect) => StartCoroutine(ResolutionCoroutine(subEffect));

    private void EffectResolutionAfterAnimation(SubEffect subEffect)
    {
        EffectsUsed effectUsed = subEffect.effectUsed;
        int effectAmount = subEffect.effectAmount;
        SetFocusCardLogic();
        EffectLogger(subEffect,targets);
        List<CardLogic> tempTargets = new(targets);

        switch (effectUsed)
        {
            //for effects that need special targeting
            case EffectsUsed.Target:
                TargetEffectLogic(subEffect);
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
                    target.EffectHandler(subEffect);
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
                    target.EffectHandler(subEffect);
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
                    target.EffectHandler(subEffect);
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
            FinishResolution(subEffect);
    }

    public void FinishResolution(SubEffect subEffect)
    {
        gameManager.isActivatingEffect = false;
        if (subEffect.parentEffect.currentActivations < subEffect.parentEffect.maxActivations)
            subEffect.parentEffect.currentActivations++;

        //chainlist for effect triggers
        gameManager.GetEffectTriggers(subEffect, this);

        CheckSubsequentEffects(subEffect);
    }

    private void CheckSubsequentEffects(SubEffect subEffect)
    {
        if (ResolveSubsequentSubeffects(subEffect))
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

    private bool ResolveSubsequentSubeffects(SubEffect subEffect)
    {
        int subCount = subEffect.parentEffect.SubEffects.FindIndex(a => a == subEffect);
        //resolve subsequent subeffects in the same effect if there is any
        if (subCount + 1 >= subEffect.parentEffect.SubEffects.Count)
            return true;

        SubEffect nextSubEffect = subEffect.parentEffect.SubEffects[subCount + 1];
        if (subEffect.effectType != nextSubEffect.effectType)
            return true;
        if (nextSubEffect.EffectActivationIsMandatory == false)
        {
            focusSubEffect = nextSubEffect;
            SetFocusCardLogic();
            if (cardController.isAI)
                OptionalEffectResolution(cardController.AIManager.ActivateOptionalEffect());
            else
                gameManager.EnableActivationPanel();
            return false;
        }
        //dependent on targets of previous effect
        if (nextSubEffect.EffectTargetAmount == 98)
            EffectResolution(nextSubEffect);
        else
        {
            targets?.Clear();
            validTargets?.Clear();
            TargetCheck(nextSubEffect);
        }
        return false;
    }

    //some effects modify effect amount based on count of something multiplied by a modifier, this handles it
    private void TargetEffectLogic(SubEffect subEffect)
    {

        List<int> effectAmountIndexesToChange = new();

        //use big E cos small e is supposed to be editable and change
        foreach(SubEffect sub in subEffect.parentEffect.SubEffects)
            if (sub.EffectAmount == 98)
                effectAmountIndexesToChange.Add(subEffect.parentEffect.SubEffects.FindIndex(a=>a==sub));
        targets[0].TryGetComponent<CombatantLogic>(out var combatantStats);
        targets[0].TryGetComponent<PlayableLogic>(out var playableStats);

        float mod = subEffect.TargetCountModifier > 0 ? subEffect.TargetCountModifier : 1;
        if (subEffect.TargetStats == null)
        {
            foreach (int index in effectAmountIndexesToChange)
            {
                subEffect.parentEffect.SubEffects[index].effectAmount = Mathf.CeilToInt(targets.Count * subEffect.TargetCountModifier);
                continue;
            }
        }
        else
        { 
            string checkedStat = subEffect.TargetStats[0];
            int index = effectAmountIndexesToChange[0];
            switch (checkedStat)
            {
                case "current atk":
                    subEffect.parentEffect.SubEffects[index].effectAmount = Mathf.CeilToInt(combatantStats.currentAtk * mod);
                    break;
                case "cost":
                    subEffect.parentEffect.SubEffects[index].effectAmount = Mathf.CeilToInt(playableStats.cost * mod);
                    break;
                default:
                    Debug.Log("unimplemented target stat");
                    break;
            }
        }
    }

    private void TargetCheck(SubEffect subEffect)
    {
        focusSubEffect = subEffect;
        if (subEffect.EffectTargetAmount == 0)
        {
            EffectResolution(subEffect);
            return;
        }

        if (targets == null || targets.Count < subEffect.effectTargetAmount)
        {
            gameManager.StateChange(GameState.Targeting);
            validTargets = new(GetValidTargets(subEffect));
            if (subEffect.targetingType == TargetingTypes.Manual)
            {
                //if no valid targets, end effect
                if (validTargets.Count == 0)
                {
                    EffectResolution(subEffect);
                    return;
                }
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
                AutoTargetAcquisition(subEffect);
                return;
            }
            if (focusSubEffect.targetingType == TargetingTypes.Random)
            {
                RandomTargetAcquisition(subEffect);
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
            EffectResolution(subEffect);
            return;
        }
    }

    //called by ux manager clicked GameObject method with current focus card logic count and subcount
    public void ManualTargetAcquisition(SubEffect subEffect)
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
        if (targeter.targets.Count == targeter.focusSubEffect.effectTargetAmount || 
            targeter.targets.Count == targeter.validTargets.Count)
        {
            gameManager.DisableCardScrollScreen();
            targeter.EffectResolution(subEffect);
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
    public void AutoTargetAcquisition(SubEffect subEffect)
    {
        focusSubEffect = subEffect;
        //auto self target effects
        if (focusSubEffect.AllowSelfTarget == true && focusSubEffect.effectTargetAmount == 1)
            targets = new() { this };
        else
            targets = new(validTargets);
        EffectResolution(subEffect);
        return;
    }

    public void RandomTargetAcquisition(SubEffect subEffect)
    {
        int targetsLeft = subEffect.effectTargetAmount;
        while (targetsLeft > 0 && validTargets.Count > targets.Count)
        {
            int randomNumber = Random.Range(0, validTargets.Count);
            if (targets.Contains(validTargets[randomNumber]))
                continue;
            targets.Add(validTargets[randomNumber]);
            targetsLeft--;
        }
        EffectResolution(subEffect);
        return;
    }

    public void OptionalEffectResolution(bool used)
    {
        if (!used)
        {
            CheckSubsequentEffects(focusSubEffect);
            return;
        }
        //if you need the targets from previous effect to resolve
        if (focusSubEffect.TargetingType != TargetingTypes.Undefined && 
            focusSubEffect.effectTargetAmount == 98)
            EffectResolution(focusSubEffect);
        else
        {
            targets.Clear();
            validTargets.Clear();
            EffectActivation(focusSubEffect);
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

    public void EffectLogger(SubEffect subEffect, List<CardLogic> cards)
    {
        Effect effect = subEffect.parentEffect;
        EffectLogHistoryEntry effectLogHistoryEntry = new(effect, subEffect.effectUsed, cards)
        {
            logIndex = gameManager.gameLogHistoryEntries.Count,
            loggedCard = this,
            loggedLocation = currentLocation
        };
        gameManager.gameLogHistoryEntries.Add(effectLogHistoryEntry);
    }

    public void EffectHandler(SubEffect subEffect)
    {
        //apart from where specified, duration set to 0(not defined) indicates an infinite duration, be careful with blanks
        CombatantLogic combatantLogic = GetComponent<CombatantLogic>();
        MonsterLogic monsterLogic = GetComponent<MonsterLogic>();
        CardLogic logic = gameManager.currentFocusCardLogic;
        int effectAmount = subEffect.EffectAmount;
        EffectsUsed effectsUsed = subEffect.effectUsed;
        Effect effect = subEffect.parentEffect;
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