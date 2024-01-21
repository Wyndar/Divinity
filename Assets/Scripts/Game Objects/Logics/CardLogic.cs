using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public string id, cardName, cardType, cardText, flavorText;

    public bool isFaceDown, isNormalColour;

    public Location currentLocation;
    public int locationOrderNumber;

    public List<CardLogic> targets = new();
    public List<CardLogic> validTargets = new();
    public List<Effect> effects = new();
    public List<BuffHistoryEntry> buffHistoryEntries = new();
    public List<DebuffHistoryEntry> debuffHistoryEntries = new();
    public List<LocationHistoryEntry> locationHistoryEntries = new();
    public List<StatChangeHistoryEntry> statChangeHistoryEnteries = new();

    public int effectCountNumber;
    public int subCountNumber;
    public Effect focusEffect;

    public float movementSpeed = 3f;

    private IEnumerator ActivationCoroutine(int effectNumber, int subEffectNumber)
    {
        gameManager.isActivatingEffect = true;
        gameManager.DisableRayBlocker();

        //these technically don't activate, they are passives
        if (effects[effectNumber].effectTypes[subEffectNumber] == EffectTypes.WhileDeployed)
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
        if (effects[effectNumber].effectTypes[subEffectNumber] == EffectTypes.WhileDeployed)
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
        if (focusEffect.targetLocation.Count == 0)
            return returnList;

        EffectsUsed effectUsed = focusEffect.effectsUsed[subEffectNumber];
        Location targetLocation = focusEffect.targetLocation[subEffectNumber];
        Controller controller = focusEffect.effectTargetController[subEffectNumber];
        Type type = Type.Undefined;
        PlayType playType = PlayType.Undefined;
        if (focusEffect.effectTargetType.Count > 0)
            type = focusEffect.effectTargetType[subEffectNumber];
        if (focusEffect.effectTargetPlayType.Count > 0)
            playType = focusEffect.effectTargetPlayType[subEffectNumber];
        foreach(CardLogic target in allTargetsList)
        {
           target.TryGetComponent<CombatantLogic>(out var combatantStats);
            target.TryGetComponent<PlayableLogic>(out var playableStats);
            // basic targeting requirements... don't target wrong location, don't target wrong owner or wrong card types
            if (target == this && focusEffect.AllowSelfTarget[subEffectNumber] == false)
                continue;
            if (target.currentLocation != targetLocation && targetLocation != Location.Undefined)
                continue;
            if (target.cardController == cardController && controller == Controller.Opponent)
                continue;
            if (target.cardController != cardController && controller == Controller.Player)
                continue;

            //successfully cleaned up the bloody nightmare code by splitting into two seperate types
            if (target.type != type && type != Type.Undefined)
                continue;
            if (!target.playTypes.Contains(playType) && playType != PlayType.Undefined)
                continue;

            // don't target max hp with healing effects
            if (effectUsed == EffectsUsed.Regeneration && (combatantStats == null || combatantStats.maxHp == combatantStats.currentHp))
                continue;
            // don't target no hp with damage effects
            if (effectUsed == EffectsUsed.Damage && (combatantStats == null || combatantStats.currentHp <= 0))
                continue;
            //don't add targets with higher cost when paying for revive or deploy cost
            if ((effectUsed == EffectsUsed.Revive|| effectUsed==EffectsUsed.Deploy) && cardController.costCount < playableStats.cost)
                continue;

            if (focusEffect.TargetStat != null)
            {
                string checkedStat = focusEffect.TargetStat[subEffectNumber];
                string condition = focusEffect.TargetStatCondition[subEffectNumber];
                int amount = focusEffect.TargetStatAmount[subEffectNumber];
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

                if(checkedStat == "cost")
                {
                    if (playableStats == null)
                        continue;
                    stat = playableStats.cost;
                }

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
            returnList.Add(target);
        }
        if (returnList.Count < 1)
            Debug.Log($"No valid targets for {cardName}'s {effectUsed} ability at {gameManager.turnPlayer.PlayerName}'s turn at phase {gameManager.currentPhase}");
        return returnList;
    }

    public void EffectActivation(int countNumber, int subCount)=>
        StartCoroutine(ActivationCoroutine(countNumber, subCount));

    private void EffectActivationAfterAnimation(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        switch (focusEffect.effectTypes[subCount])
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
                if (currentLocation == enumConverter.LocationStringToEnum(focusEffect.ActivationLocation))
                    TargetCheck(countNumber, subCount);
                break;
        }
        SetFocusCardLogic();
    }

    public void EffectResolution(int countNumber, int subCount) =>StartCoroutine(ResolutionCoroutine(countNumber, subCount));

    private void EffectResolutionAfterAnimation(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        EffectsUsed effectUsed = focusEffect.effectsUsed[subCount];
        int effectAmount = focusEffect.EffectAmount[subCount];
        effectCountNumber = countNumber;
        subCountNumber = subCount;
        SetFocusCardLogic();

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
                foreach (CardLogic target in targets)
                    StartCoroutine(gameManager.SearchCard(target, target.cardController));
                break;
            case EffectsUsed.Recover:
                foreach (CardLogic target in targets)
                    StartCoroutine(gameManager.RecoverCard(target, cardController));
                break;

            //these are undefined effects, causes minor bugs, might need recursion
            case EffectsUsed.Damage:
            case EffectsUsed.Regeneration:
            case EffectsUsed.Shatter:
            case EffectsUsed.Detonate:
            case EffectsUsed.BurnDetonate:
            case EffectsUsed.BombDetonate:
            case EffectsUsed.PoisonDetonate:
                foreach (CardLogic target in targets)
                    target.EffectHandler(effectUsed, effectAmount, LogType.Undefined, focusEffect);
                break;


            //these are buffs
            case EffectsUsed.Rally:
            case EffectsUsed.Vigor:
            case EffectsUsed.Taunt:
            case EffectsUsed.Stealth:
            case EffectsUsed.Armor:
            case EffectsUsed.Camouflage:
                foreach (CardLogic target in targets)
                    target.EffectHandler(effectUsed, effectAmount, LogType.Buff, focusEffect);
                break;

            //these are debuffs
            case EffectsUsed.Terrify:
            case EffectsUsed.Intimidate:
            case EffectsUsed.Weaken:
            case EffectsUsed.Sleep:
            case EffectsUsed.Stun:
            case EffectsUsed.Provoke:
            case EffectsUsed.Blind:
            case EffectsUsed.Burn:
            case EffectsUsed.Poison:
            case EffectsUsed.Bomb:
            case EffectsUsed.Spot:
            case EffectsUsed.Bounce:
                foreach (CardLogic target in targets)
                    target.EffectHandler(effectUsed, effectAmount, LogType.Debuff, focusEffect);
                break;
            case EffectsUsed.FreeRevive:
            case EffectsUsed.Revive:
            case EffectsUsed.FreeDeploy:
            case EffectsUsed.Deploy:
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard(effectUsed, cardController);
                break;
            default:
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
            if (cardType == "spell")
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

        //resolve subsequent subeffects in the same effect if there is any
        if (focusEffect.effectsUsed.Count <= subCount + 1)
            return true;
        if (focusEffect.effectTypes[subCount] != focusEffect.effectTypes[subCount + 1])
            return true;
        if (focusEffect.EffectActivationIsMandatory[subCount + 1] == false)
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
        if (focusEffect.EffectTargetAmount[subCount + 1] == 98)
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
        int[] effectAmountIndexesToChange = focusEffect.EffectAmount.FindAllIndexof(98);
        targets[0].TryGetComponent<CombatantLogic>(out var combatantStats);
        targets[0].TryGetComponent<PlayableLogic>(out var playableStats);

        foreach (int index in effectAmountIndexesToChange)
        {
            if (focusEffect.TargetCountModifier != null && focusEffect.TargetCountModifier[subCount]!=0)
            {
                focusEffect.EffectAmount[index] = Mathf.CeilToInt(targets.Count * focusEffect.TargetCountModifier[subCount]);
                continue;
            }

            string checkedStat = focusEffect.TargetStat[subCount];
            switch (checkedStat)
            {
                case "current atk":
                    focusEffect.EffectAmount[index] = combatantStats.currentAtk;
                    break;
                case "cost":
                    focusEffect.EffectAmount[index] = playableStats.cost;
                    break;
            }
        }
    }

    private void TargetCheck(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        if (focusEffect.EffectTargetAmount == null)
        {
            EffectResolution(countNumber, subCount);
            return;
        }
        if (focusEffect.EffectTargetAmount[subCount] == 0)
        {
            EffectResolution(countNumber, subCount);
            return;
        }

        if (targets == null || targets.Count < focusEffect.EffectTargetAmount[subCount])
        {
            gameManager.StateChange(GameState.Targeting);
            validTargets = new(GetValidTargets(countNumber, subCount));
            if (focusEffect.targetingTypes[subCount] == TargetingTypes.Manual)
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
                foreach(CardLogic target in validTargets)
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
                if(cardController.isAI)
                {
                    cardController.AIManager.GetEffectTarget();
                    return;
                }
                if (focusEffect.targetLocation[subCount] != Location.Field)
                    gameManager.EnableCardScrollScreen(validTargets, !focusEffect.EffectActivationIsMandatory[subCount]);
                return;
            }
            if (focusEffect.targetingTypes[subCount] == TargetingTypes.Auto)
            {
                AutoTargetAcquisition(countNumber, subCount);
                return;
            }
            if (focusEffect.targetingTypes[subCount] == TargetingTypes.Random)
            {
                RandomTargetAcquisition(countNumber, subCount);
                return;
            }
            if (focusEffect.targetingTypes[countNumber] == TargetingTypes.Trigger)
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
        if (targeter.targets.Count == targeter.effects[countNumber].EffectTargetAmount[subCount] || targeter.targets.Count == targeter.validTargets.Count)
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
        //auto self target effects
        if (focusEffect.AllowSelfTarget[subCount] == true && focusEffect.EffectTargetAmount[subCount] == 1)
            targets = new() { this};
        else
            targets = new(validTargets);
        EffectResolution(countNumber, subCount);
        return;
    }

    public void RandomTargetAcquisition(int countNumber, int subCount)
    {
        int targetsLeft = effects[countNumber].EffectTargetAmount[subCount];
        while (targetsLeft > 0 && validTargets.Count>targets.Count)
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
        if(!used)
        {
            CheckSubsequentEffects(effectCountNumber, subCountNumber);
            return;
        }
        //if you need the targets from previous effect to resolve
        if (effects[effectCountNumber].TargetingType != null && effects[effectCountNumber].EffectTargetAmount[subCountNumber] == 98)
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
        if(gameManager.currentFocusCardLogic != null)
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
    public void LocationChange(Effect effect, EffectsUsed effectsUsed, Location location, int num)
    {
        LocationHistoryEntry locationLog = new(location, currentLocation)
        {
            log = LogType.Location,
            logIndex = locationHistoryEntries.Count,
            loggedCard = this,
            loggedEffect=effect,
            loggedEffectUsed=effectsUsed
        };
        locationHistoryEntries.Add(locationLog);
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

    virtual public void StatAdjustment(int value, Status status)=> Debug.Log($"Failed virtual override for status of {cardName}");

    public void EffectLogger(EffectsUsed effectsUsed, int effectAmount, LogType logType, Effect effect)
    {
        if (logType == LogType.Buff)
        {
            BuffHistoryEntry buffHistoryEntry = new(effectAmount, currentLocation)
            {
                log = logType,
                logIndex = buffHistoryEntries.Count,
                loggedCard = this,
                loggedEffect = effect,
                loggedEffectUsed = effectsUsed,
            };
            buffHistoryEntries.Add(buffHistoryEntry);
            gameManager.gameLogHistoryEntries.Add(buffHistoryEntry);
        }
        else if(logType==LogType.Debuff)
        {
            DebuffHistoryEntry debuffHistoryEntry = new(effectAmount, currentLocation)
            {
                log = logType,
                logIndex = debuffHistoryEntries.Count,
                loggedCard = this,
                loggedEffect = effect,
                loggedEffectUsed = effectsUsed,
            };
            debuffHistoryEntries.Add(debuffHistoryEntry);
            gameManager.gameLogHistoryEntries.Add(debuffHistoryEntry);
        }
    }

    public void EffectHandler(EffectsUsed effectsUsed, int effectAmount, LogType logType, Effect effect)
    {
        CombatantLogic combatantLogic = GetComponent<CombatantLogic>();
        MonsterLogic monsterLogic = GetComponent<MonsterLogic>();
        CardLogic logic = gameManager.currentFocusCardLogic;
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
                Taunt taunt = new(logic, this, false,0);
                combatantLogic.SetTargetStatus(taunt, TargetState.Taunt);
                cardController.SetStatusIcon(locationOrderNumber, taunt);
                break;
            case EffectsUsed.Stealth:
                Stealth stealth = new(logic, this, false, 0);
                combatantLogic.SetTargetStatus(stealth, TargetState.Stealth);
                cardController.SetStatusIcon(locationOrderNumber, stealth);
                break;
            case EffectsUsed.Camouflage:
                combatantLogic.targetState = TargetState.Camouflage;
                break;
            case EffectsUsed.Armor:
                combatantLogic.armor = effectAmount;
                Armor armor = new(logic, this, effectAmount);
                combatantLogic.cardStatuses.Add(armor);
                cardController.SetStatusIcon(locationOrderNumber, armor);
                break;
            case EffectsUsed.Sleep:
                break;
            case EffectsUsed.Stun:
                break;
            case EffectsUsed.Provoke:
                break;
            case EffectsUsed.Blind:
                break;
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
            case EffectsUsed.Spot:
                break;
            case EffectsUsed.Bounce:
                StartCoroutine(monsterLogic.BounceCard());
                break;
            case EffectsUsed.Detonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    status.DetonateActions(gameManager);
                break;
            case EffectsUsed.BombDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Bomb)
                        status.DetonateActions(gameManager);
                break;
            case EffectsUsed.BurnDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Burn)
                        status.DetonateActions(gameManager);
                break;
            case EffectsUsed.PoisonDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Poison)
                        status.DetonateActions(gameManager);
                break;
            default:
                return;
        }
        EffectLogger(effectsUsed, effectAmount, logType, effect);
    }
}