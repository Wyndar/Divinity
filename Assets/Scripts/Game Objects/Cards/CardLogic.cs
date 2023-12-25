using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardLogic : MonoBehaviour
{
    public Game_Manager G_M;

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
        G_M.isActivatingEffect = true;
        G_M.DisableRayBlocker();

        //these technically don't activate
        if (effects[effectNumber].effectTypes[subEffectNumber] == EffectTypes.WhileDeployed)
        {
            EffectActivationAfterAnimation(effectNumber, subEffectNumber);
            yield break;
        }

        G_M.StateChange(GameState.EffectActivation);
        float distance = Vector3.Distance(transform.position, cardController.activationZone.position);
        Vector3 originalPosition = transform.position;
        Vector3 direction = (cardController.activationZone.position - transform.position).normalized;
        float distanceTravelled = 0;
        while (distanceTravelled < distance)
        {
            Vector3 translationDistance = (cardController.activationZone.position - transform.position);
            if (translationDistance.magnitude <= direction.magnitude)
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
        G_M.DisableRayBlocker();

        //these technically don't resolve
        if (effects[effectNumber].effectTypes[subEffectNumber] == EffectTypes.WhileDeployed)
        {
            EffectResolutionAfterAnimation(effectNumber, subEffectNumber);
            yield break;
        }

        G_M.StateChange(GameState.EffectResolution);
        float distance = Vector3.Distance(transform.position, cardController.activationZone.position);
        Vector3 originalPosition = transform.position;
        Vector3 direction = (cardController.activationZone.position - transform.position).normalized;
        float distanceTravelled = 0;
        while (distanceTravelled < distance)
        {
            Vector3 translationDistance = (cardController.activationZone.position - transform.position);
            if (translationDistance.magnitude <= direction.magnitude)
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
            Debug.Log("No valid targets for "+ cardName+"'s "+ effectUsed+ " ability at "+ G_M.turnPlayer.PlayerName+"'s turn at phase " + G_M.currentPhase);
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
        G_M.currentFocusCardLogic = this;
    }

    public void EffectResolution(int countNumber, int subCount) =>StartCoroutine(ResolutionCoroutine(countNumber, subCount));

    private void EffectResolutionAfterAnimation(int countNumber, int subCount)
    {
        focusEffect = effects[countNumber];
        EffectsUsed effectUsed = focusEffect.effectsUsed[subCount];
        int effectAmount = focusEffect.EffectAmount[subCount];
        effectCountNumber = countNumber;
        subCountNumber = subCount;
        G_M.currentFocusCardLogic = this;

        switch (effectUsed)
        {
            //for effects that need special targeting
            case EffectsUsed.Target:
                TargetEffectLogic(countNumber, subCount);
                break;

            //effects that access game manager methods, can be optimized further
            case EffectsUsed.Reinforce:
                StartCoroutine(G_M.DrawCard(effectAmount, cardController));
                break;
            case EffectsUsed.BloodRecovery:
                G_M.CostChange(effectAmount, cardController, true);
                break;
            case EffectsUsed.Recruit:
                foreach (CardLogic target in targets)
                    StartCoroutine(G_M.SearchCard(target, target.cardController));
                break;
            case EffectsUsed.Recover:
                foreach (CardLogic target in targets)
                    StartCoroutine(G_M.RecoverCard(target, cardController));
                break;

            //these are undefined effects
            case EffectsUsed.Damage:
            case EffectsUsed.Regeneration:
            case EffectsUsed.Shatter:
            case EffectsUsed.Detonate:
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
        if (!G_M.isWaitingForResponse)
            FinishResolution(countNumber, subCount);
    }

    public void FinishResolution(int countNumber, int subCount)
    {
        focusEffect = this.effects[countNumber];
        G_M.isActivatingEffect = false;
        if (focusEffect.currentActivations < focusEffect.maxActivations)
            focusEffect.currentActivations++;

        //chainlist for effect triggers
        G_M.GetEffectTriggers(countNumber, subCount, this);

        CheckSubsequentEffects(countNumber, subCount);
    }

    private void CheckSubsequentEffects(int countNumber, int subCount)
    {
        if (ResolveSubsequentSubeffects(countNumber, subCount))
        {
            G_M.ClearEffectTargetImages();
            targets?.Clear();
            validTargets?.Clear();
            if (cardType == "spell")
                gameObject.GetComponent<PlayableLogic>().MoveToGrave();

            //resolve chain after all possible effect chains are linked
            if (cardController.isAI)
                cardController.AIManager.isPerformingAction = false;
            G_M.ChainResolution();
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
            G_M.currentFocusCardLogic = this;
            if (cardController.isAI)
                OptionalEffectResolution(cardController.AIManager.ActivateOptionalEffect());
            else
                G_M.EnableActivationPanel();
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
            int amount = focusEffect.EffectAmount[index];
            switch (checkedStat)
            {
                case "current atk":
                    combatantStats.currentAtk = amount;
                    break;
                case "cost":
                    playableStats.cost = amount;
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
            G_M.StateChange(GameState.Targeting);
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
                G_M.currentFocusCardLogic = this;
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
                    G_M.EnableCardScrollScreen(validTargets, !focusEffect.EffectActivationIsMandatory[subCount]);
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
                targets = new() { G_M.currentFocusCardLogic };
                return;
            }
        }
        if (G_M.gameState != GameState.Targeting && targets.Count == 0)
        {
            EffectResolution(countNumber, subCount);
            return;
        }
    }

    //called by ux manager clicked GameObject method with current focus card logic count and subcount
    public void ManualTargetAcquisition(int countNumber, int subCount)
    {
        if (G_M.gameState != GameState.Targeting)
            return;

        CardLogic targeter = G_M.currentFocusCardLogic;
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
            G_M.DisableCardScrollScreen();
            targeter.EffectResolution(countNumber, subCount);
            return;
        }
    }

    public void ManualTargetRemoval()
    {
        if (G_M.gameState != GameState.Targeting)
            return;
        CardLogic targeter = G_M.currentFocusCardLogic;
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
        G_M.gameLogHistoryEntries.Add(locationLog);
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
            G_M.gameLogHistoryEntries.Add(buffHistoryEntry);
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
            G_M.gameLogHistoryEntries.Add(debuffHistoryEntry);
        }
    }

    public void EffectHandler(EffectsUsed effectsUsed, int effectAmount, LogType logType, Effect effect)
    {
        CombatantLogic combatantLogic = GetComponent<CombatantLogic>();
        MonsterLogic monsterLogic = GetComponent<MonsterLogic>();
        CardLogic logic = G_M.currentFocusCardLogic;
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
                combatantLogic.targetState = TargetState.Taunt;
                break;
            case EffectsUsed.Stealth:
                combatantLogic.targetState = TargetState.Stealth;
                break;
            case EffectsUsed.Camouflage:
                combatantLogic.targetState = TargetState.Camouflage;
                break;
            case EffectsUsed.Armor:
                combatantLogic.armor = effectAmount;
                GameObject icon = cardController.armorIcons[locationOrderNumber];
                icon.SetActive(true);
                icon.GetComponentInChildren<TMP_Text>().text = effectAmount.ToString();
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
                break;
            case EffectsUsed.Poison:
                break;
            case EffectsUsed.Bomb:
                //bombs have a default timer of three turns
                cardController.SetDebuffIcon(locationOrderNumber, Debuffs.Bombed);
                Debuff bomb = new(Debuffs.Bombed, logic, this, effect.duration, true,G_M.ToolTipManager.tooltipInfos.Find(a=>a.key=="Bomb"), U_I.bombSprite);
                combatantLogic.cardStatuses.Add(bomb);
                break;
            case EffectsUsed.Spot:
                break;
            case EffectsUsed.Bounce:
                break;
            case EffectsUsed.Detonate:
                break;
            default:
                return;
        }
        EffectLogger(effectsUsed, effectAmount, logType, effect);
    }
}