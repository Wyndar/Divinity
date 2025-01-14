using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardLogic : MonoBehaviour
{
    public GameBattleManager gameManager;

    public AudioManager audioManager;

    public SecondaryUIManager U_I;

    public Type type;
    public Rarity rarity;
    public List<PlayType> playTypes = new();
    public List<Trait> traits = new();
    public List<Attunement> attunements = new();

    public PlayerManager cardOwner, cardController;
    public Transform cardBack, cardImage, cardImageBorder, cardOutline, textCanvas, EnergyIcon;
    public Sprite image;
    public TMP_Text costText, ATKText, HPText;

    public string id, cardName, cardText, flavorText;

    public bool isFaceDown, isNormalColour;

    public Location currentLocation;
    public int locationOrderNumber;

    public List<CardLogic> targets = new();
    public List<CardLogic> validTargets = new();
    public List<Effect> effects = new();

    public SubEffect focusSubEffect;

    public float movementSpeed = 3f;

    public void EffectActivation(SubEffect subEffect)
    {
        if (subEffect.effectUsed == EffectsUsed.BloodCost &&
    cardController.BloodAttunementCheck(Enum.Parse<Attunement>(subEffect.TargetStats[0])) < subEffect.effectAmount)
            return;
        gameManager.isActivatingEffect = true;
        gameManager.DisableRayBlocker();
        //these technically don't activate, they are passives
        if (subEffect.effectType != EffectTypes.WhileDeployed)
        {
            gameManager.StateChange(GameState.EffectActivation);
            audioManager.NewAudioPrefab(audioManager.effectActivation);
            if (type != Type.God)
            {
                StartCoroutine(TweenAnimationCoroutine(subEffect, true));
                return;
            }
        }
        EffectActivationAfterAnimation(subEffect);
    }

    public void EffectResolution(SubEffect subEffect)
    {
        gameManager.DisableRayBlocker();
        //these technically don't resolve
        if (subEffect.effectType != EffectTypes.WhileDeployed)
        {
            gameManager.StateChange(GameState.EffectResolution);
            if (type != Type.God)
            {
                StartCoroutine(TweenAnimationCoroutine(subEffect, false));
                return;
            }
        }
        EffectResolutionAfterAnimation(subEffect);
    }

    private IEnumerator TweenAnimationCoroutine(SubEffect subEffect, bool isActivation)
    {
        //tween and grow then shrink animation
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
        transform.localScale = new(originalScale.x * 2.5f, originalScale.y * 2.5f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        if (isActivation)
            EffectActivationAfterAnimation(subEffect);
        else
            EffectResolutionAfterAnimation(subEffect);
        yield break;
    }

    public List<CardLogic> GetValidTargets(SubEffect subEffect, bool shouldShowError)
    {
        List<CardLogic> allTargetsList = new(FindObjectsByType<CardLogic>(FindObjectsSortMode.None));
        List<CardLogic> returnList = new();
        if (subEffect.targetLocations.Count == 0)
            return returnList;

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

            if (subEffect.effectTargetTypes != null && !subEffect.effectTargetTypes.Contains(target.type))
                continue;

            // don't target max hp with healing effects
            if (effectUsed == EffectsUsed.Regeneration && (combatantStats == null || combatantStats.maxHp == combatantStats.currentHp))
                continue;
            // don't target no hp with damage effects
            if (effectUsed == EffectsUsed.Damage && (combatantStats == null || combatantStats.currentHp <= 0))
                continue;
            //don't add targets with higher cost when paying for revive or deploy cost
            //needs attunement check fix
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
            if (subEffect.TargetStats == null)
                returnList.Add(target);
            else
                TargetStatCheck(subEffect, returnList, target, combatantStats, playableStats);
        }
        List<CardLogic> camoTargets = GetCamoTargets(allTargetsList, true);
        if (allTargetsList.Count == 0)
            allTargetsList.AddRange(camoTargets);
        if (returnList.Count < 1 && shouldShowError)
            gameManager.ErrorCodePanel($"There are no valid targets for {cardName}'s {effectUsed} ability.");
        return returnList;
    }

    private static List<CardLogic> GetCamoTargets(List<CardLogic> allTargetsList, bool removeFromList)
    {
        List<CardLogic> camoTargets = new();
        foreach (CardLogic logic in allTargetsList)
        {
            logic.TryGetComponent<CombatantLogic>(out var combatantLogic);
            if (logic.currentLocation == Location.Field)
                if (combatantLogic.targetState == TargetState.Camouflage)
                {
                    camoTargets.Add(logic);
                    if (removeFromList)
                        allTargetsList.Remove(logic);
                }
        }
        return camoTargets;
    }

    private static void TargetStatCheck(SubEffect subEffect, List<CardLogic> returnList, CardLogic target, CombatantLogic combatantStats, PlayableLogic playableStats)
    {
        for (int i = 0; i < subEffect.TargetStats.Count; i++)
        {
            string checkedStat = subEffect.TargetStats[i];
            string condition = subEffect.TargetStatConditions[i];
            int stat = 0;
            //as more types of effects are added, more checks will be needed
            switch (checkedStat)
            {
                case "none":
                    returnList.Add(target);
                    continue;
                case "current atk":
                    if (combatantStats == null)
                        continue;
                    stat = combatantStats.currentAtk;
                    break;
                case "cost":
                    if (playableStats == null)
                        continue;
                    stat = playableStats.cost;
                    break;
            }
            if (checkedStat == "name")
            {
                if (condition[..3] == "has" && !target.cardName.Contains(condition[3..]))
                    continue;
                if (condition[..4] == "nhas" && target.cardName.Contains(condition[4..]))
                    continue;
                if (condition[..2] == "is" && target.cardName != condition[2..])
                    continue;
                if (condition[..3] == "not" && target.cardName == condition[3..])
                    continue;
            }
            if (subEffect.TargetStatAmounts != null)
            {
                int amount = subEffect.TargetStatAmounts[i];
                // don't target stats not equal requirements
                switch (condition)
                {
                    case "==" when stat != amount:
                    case ">=" when stat < amount:
                    case "<=" when stat > amount:
                    case ">" when stat <= amount:
                    case "<" when stat >= amount:
                        continue;
                }
            }
            returnList.Add(target);
        }
    }

    private void EffectActivationAfterAnimation(SubEffect subEffect)
    {
        focusSubEffect = subEffect;
        if (subEffect.parentEffect.activationLocations == null || subEffect.parentEffect.activationLocations.Contains(currentLocation))
            TargetCheck(subEffect);
    }

    private void EffectResolutionAfterAnimation(SubEffect subEffect)
    {
        EffectsUsed effectUsed = subEffect.effectUsed;
        int effectAmount = subEffect.effectAmount;
        SetFocusCardLogic();
        EffectLogger(subEffect, targets);
        List<CardLogic> tempTargets = new(targets);

        switch (effectUsed)
        {
            //for effects that need special targeting
            case EffectsUsed.Target:
                TargetEffectLogic(subEffect);
                break;
            case EffectsUsed.BloodCost:
                List<Attunement> sendList = new()
                {
                    Enum.Parse<Attunement>(subEffect.TargetStats[0])
                };
                cardController.BloodLoss(sendList, effectAmount);
                break;
            //effects that access game manager methods, can be optimized further
            case EffectsUsed.Reinforce:
                StartCoroutine(gameManager.DrawCard(effectAmount, cardController));
                break;
            case EffectsUsed.BloodRecovery:
                cardController.BloodGain(Attunement.Untuned, effectAmount);
                break;
            case EffectsUsed.Recruit:
                StartCoroutine(gameManager.SearchCard(tempTargets, cardController, this));
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
                throw new MissingReferenceException("Attempting to use an unimplemented effect");
        }
        if (!gameManager.isWaitingForResponse)
            FinishResolution(subEffect);
    }

    public void FinishResolution(SubEffect subEffect)
    {
        gameManager.GetEffectTriggers(subEffect, this);
        CheckSubsequentEffects(subEffect, true);
    }

    private void CheckSubsequentEffects(SubEffect subEffect, bool resolvedPreviousSubEffect)
    {
        if (!ResolveSubsequentSubeffects(subEffect))
            return;
        if (resolvedPreviousSubEffect && subEffect.parentEffect.currentActivations < subEffect.parentEffect.maxActivations)
            subEffect.parentEffect.currentActivations++;
        gameManager.ClearEffectTargetImages();
        targets?.Clear();
        validTargets?.Clear();
        if (type == Type.Spell)
            GetComponent<PlayableLogic>().MoveToGrave();
        audioManager.NewAudioPrefab(audioManager.effectResolution);
        //resolve chain after all possible effect chains are linked
        if (cardController.isAI)
            cardController.AIManager.isPerformingAction = false;
        gameManager.isActivatingEffect = false;
        gameManager.ChainResolution();
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
        //any sub effect using same targets must be mandatory
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
        //uses the same target(s) of previous sub effect
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
        int subEffectIndex = subEffect.parentEffect.SubEffects.FindIndex(a => a == subEffect);
        //use big E cos small e is supposed to be editable and change
        foreach (SubEffect sub in subEffect.parentEffect.SubEffects)
        {
            int subIndex = subEffect.parentEffect.SubEffects.FindIndex(a => a == sub);
            if (sub.DependentEffectParameters == null || subIndex <= subEffectIndex)
                continue;
            float mod = sub.TargetCountModifier > 0 ? sub.TargetCountModifier : 1;
            foreach (DependentEffectParameter dependent in sub.DependentEffectParameters)
            {
                int targetVariableIndex = sub.DependentEffectParameters.FindIndex(a => a == dependent);
                if (targetVariableIndex != -1 && sub.DependentIndices[targetVariableIndex] == subEffectIndex)
                    switch (dependent)
                    {
                        case DependentEffectParameter.EffectTargetAmount:
                            sub.effectTargetAmount = subEffect.TargetStats == null
                            ? Mathf.FloorToInt(targets.Count * mod)
                            : GetModifiedDepenndentParameterValue(subEffect.TargetStats[targetVariableIndex], mod);
                            break;
                        case DependentEffectParameter.EffectAmount:
                            sub.effectAmount = subEffect.TargetStats == null
                            ? Mathf.FloorToInt(targets.Count * mod)
                            : GetModifiedDepenndentParameterValue(subEffect.TargetStats[targetVariableIndex], mod);
                            break;
                        default:
                            throw new MissingReferenceException("Unimplemented dependent parameter required for effect");
                    }
            }
        }
    }

    private int GetModifiedDepenndentParameterValue(string checkedStat, float mod)
    {
        targets[0].TryGetComponent<CombatantLogic>(out var combatant);
        targets[0].TryGetComponent<PlayableLogic>(out var playable);
        return checkedStat switch
        {
            "current atk" => Mathf.CeilToInt(combatant.currentAtk * mod),
            "cost" => Mathf.CeilToInt(playable.cost * mod),
            _ => throw new MissingReferenceException("unimplemented target stat"),
        };
    }

    private void TargetCheck(SubEffect subEffect)
    {
        focusSubEffect = subEffect;
        //small e ffs, caps is data and the changed variable is what we need to check for activations to allow effect changes
        if (subEffect.effectTargetAmount == 0)
        {
            EffectResolution(subEffect);
            return;
        }

        if (targets == null || targets.Count < subEffect.effectTargetAmount)
        {
            gameManager.StateChange(GameState.Targeting);
            validTargets = new(GetValidTargets(subEffect, true));
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
                    if (target is MonsterLogic monster && target.currentLocation == Location.Field)
                        monster.currentSlot.effectTarget.SetActive(true);
                    if (target is GodLogic god && target.currentLocation == Location.Field)
                        god.cardController.heroEffectTarget.SetActive(true);
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
                gameManager.EnableCardScrollScreen(validTargets, !focusSubEffect.EffectActivationIsMandatory);
                return;
            }
            switch (focusSubEffect.targetingType)
            {
                case TargetingTypes.Auto:
                    AutoTargetAcquisition(subEffect);
                    return;
                case TargetingTypes.Random:
                    RandomTargetAcquisition(subEffect);
                    return;
                case TargetingTypes.Trigger:
                    targets = new();
                    return;
            }
        }
        if (gameManager.gameState != GameState.Targeting && targets.Count == 0)
            EffectResolution(subEffect);
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
            int randomNumber = UnityEngine.Random.Range(0, validTargets.Count);
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
            CheckSubsequentEffects(focusSubEffect, used);
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
        if (type != Type.God)
            ToggleCardOutline(true);
    }

    public void RemoveFocusCardLogic()
    {
        gameManager.currentFocusCardLogic = null;
        if (type != Type.God)
            ToggleCardOutline(false);
    }

    public void Flip(bool facedown)
    {
        isFaceDown = facedown;
        cardBack.gameObject.SetActive(facedown);
        cardImage.gameObject.SetActive(!facedown);
        cardImageBorder.gameObject.SetActive(!facedown);
        textCanvas.gameObject.SetActive(!facedown);
        audioManager.NewAudioPrefab(audioManager.flipCard);
    }

    public void ToggleCardOutline(bool on)
    {
        if (type == Type.God)
            return;
        if (currentLocation != Location.Grave)
            cardOutline.gameObject.SetActive(on);
        else
            cardOwner.underworldManager.outline.SetActive(on);
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

    public void GreyScaleEffect(bool activate)
    {
        isNormalColour = !activate;
        cardImage.GetComponent<SpriteRenderer>().color = activate ? Color.grey : Color.white;
        cardImageBorder.GetComponent<SpriteRenderer>().color = activate ? Color.grey : Color.white;
    }

    public void ControllerSwap(PlayerManager player)
    {
        cardController = player;
        gameObject.transform.rotation = player.deck.transform.rotation;
    }

    public virtual void StatAdjustment(int value, Status status) => throw new MissingReferenceException($"Failed virtual override for status of {cardName}");

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
        int duration = subEffect.duration;
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
                Taunt taunt = new(logic, this, duration);
                combatantLogic.SetTargetStatus(taunt, TargetState.Taunt);
                break;
            case EffectsUsed.Stealth:
                Stealth stealth = new(logic, this, duration);
                combatantLogic.SetTargetStatus(stealth, TargetState.Stealth);
                break;
            //everything above here works fine
            case EffectsUsed.Camouflage:
                Camouflage camouflage = new(logic, this, duration);
                combatantLogic.SetTargetStatus(camouflage, TargetState.Stealth);
                break;
            //everything from here works fine
            case EffectsUsed.Armor:
                Armor armor = new(logic, this, effectAmount, duration);
                combatantLogic.AddNonStackingBuff(armor);
                break;
            case EffectsUsed.Barrier:
                Barrier barrier = new(logic, this, effectAmount, duration);
                combatantLogic.AddNonStackingBuff(barrier);
                break;
            //till here
            case EffectsUsed.Sleep:
                Sleep sleep = new(logic, this, duration);
                combatantLogic.AddNonStackingDebuff(sleep);
                break;
            case EffectsUsed.Stun:
                Stun stun = new(logic, this, duration);
                combatantLogic.AddNonStackingDebuff(stun);
                break;
            case EffectsUsed.Provoke:
                Provoke provoke = new(logic, this, duration);
                combatantLogic.SetTargetStatus(provoke, TargetState.Stealth);
                break;
            case EffectsUsed.Disarm:
                Disarm disarm = new(logic, this, duration);
                combatantLogic.AddNonStackingDebuff(disarm);
                break;
            //everything from here works fine
            case EffectsUsed.Burn:
                //burns have a default timer of two turns, if duration is set to 0/not defined(int), default applies
                Burn burn = new(logic, this, duration);
                combatantLogic.cardStatuses.Add(burn);
                GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(burn);
                break;
            case EffectsUsed.Poison:
                //poisons have a default timer of four turns, if duration is set to 0/not defined(int), default applies
                Poison poison = new(logic, this, duration);
                combatantLogic.cardStatuses.Add(poison);
                GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(poison);
                break;
            case EffectsUsed.Bomb:
                //bombs have a default timer of three turns, if duration is set to 0/not defined(int), default applies
                Bomb bomb = new(logic, this, duration);
                combatantLogic.cardStatuses.Add(bomb);
                GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(bomb);
                break;
            //till here
            case EffectsUsed.Spot:
                Spot spot = new(logic, this, duration);
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
                Silence silence = new(logic, this, duration);
                combatantLogic.AddNonStackingDebuff(silence);
                break;
            case EffectsUsed.BloodBoost:
                break;
            case EffectsUsed.Advancement:
                break;
            case EffectsUsed.Knockback:
                break;
            case EffectsUsed.Pull:
                break;
            case EffectsUsed.Shift:
                break;
            case EffectsUsed.Immortality:
                break;
            case EffectsUsed.Immunity:
                break;
            case EffectsUsed.Spin:
                break;
            case EffectsUsed.Discard:
                break;
            case EffectsUsed.Mill:
                break;
            case EffectsUsed.ForceRetreat:
                break;
            case EffectsUsed.Return:
                break;
            case EffectsUsed.Retreat:
                break;
            case EffectsUsed.DeathCurse:
                break;
            case EffectsUsed.Mark:
                break;
            case EffectsUsed.Freeze:
                break;
            default:
                throw new MissingReferenceException("effect not found");
        }
    }
    public bool IsValidEffect()
    {
        if (effects != null)
            foreach (Effect effect in effects)
                if (effect.currentActivations < effect.maxActivations && effect.SubEffects[0].effectType == EffectTypes.Deployed)
                    if (effect.SubEffects[0].effectUsed != EffectsUsed.BloodCost ||
                        cardController.BloodAttunementCheck(Enum.Parse<Attunement>(effect.SubEffects[0].TargetStats[0])) >= effect.SubEffects[0].effectAmount)
                        return true;
        return false;
    }
}