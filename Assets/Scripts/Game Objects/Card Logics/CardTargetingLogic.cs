using System.Collections.Generic;
using UnityEngine;

public class CardTargetingLogic: MonoBehaviour
{
    public List<CardLogic> targets = new();
    public List<CardLogic> validTargets = new();
    private CardLogic cardLogic;
    public void Initialize() => cardLogic = GetComponent<CardLogic>();
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
            if (subEffect.targetLocations != null && !subEffect.targetLocations.Contains(target.dataLogic.currentLocation))
                continue;
            if (target.dataLogic.cardController == cardLogic.dataLogic.cardController && controller == Controller.Opponent)
                continue;
            if (target.dataLogic.cardController != cardLogic.dataLogic.cardController && controller == Controller.Player)
                continue;

            if (subEffect.effectTargetTypes != null && !subEffect.effectTargetTypes.Contains(target.dataLogic.type))
                continue;

            // don't target max hp with healing effects
            if (effectUsed == EffectsUsed.Regeneration && (combatantStats == null || combatantStats.maxHp == combatantStats.currentHp))
                continue;
            // don't target no hp with damage effects
            if (effectUsed == EffectsUsed.Damage && (combatantStats == null || combatantStats.currentHp <= 0))
                continue;
            //don't add targets with higher cost when paying for revive or deploy cost
            //needs attunement check fix
            if ((effectUsed == EffectsUsed.Revive || effectUsed == EffectsUsed.Deploy) && cardLogic.dataLogic.cardController.costCount < playableStats.cost)
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
            cardLogic.gameManager.ErrorCodePanel($"There are no valid targets for {cardLogic.dataLogic.cardName}'s {effectUsed} ability.");
        return returnList;
    }

    private static List<CardLogic> GetCamoTargets(List<CardLogic> allTargetsList, bool removeFromList)
    {
        List<CardLogic> camoTargets = new();
        foreach (CardLogic logic in allTargetsList)
        {
            logic.TryGetComponent<CombatantLogic>(out var combatantLogic);
            if (logic.dataLogic.currentLocation == Location.Field)
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
            if (checkedStat == "name" && ((condition[..3] == "has" && !target.dataLogic.cardName.Contains(condition[3..]))
                || (condition[..4] == "nhas" && target.dataLogic.cardName.Contains(condition[4..])) 
                || (condition[..2] == "is" && target.dataLogic.cardName != condition[2..]) 
                || (condition[..3] == "not" && target.dataLogic.cardName == condition[3..])))
                continue;
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


    public void TargetCheck(SubEffect subEffect)
    {
        cardLogic.effectLogic.focusSubEffect = subEffect;
        //small e ffs, caps is data and the changed variable is what we need to check for activations to allow effect changes
        if (subEffect.effectTargetAmount == 0)
        {
            cardLogic.EffectResolution(subEffect);
            return;
        }

        if (targets == null || targets.Count < subEffect.effectTargetAmount)
        {
            cardLogic.gameManager.StateChange(GameState.Targeting);
            validTargets = new(GetValidTargets(subEffect, true));
            if (subEffect.targetingType == TargetingTypes.Manual)
            {
                //if no valid targets, end effect
                if (validTargets.Count == 0)
                {
                    cardLogic.EffectResolution(subEffect);
                    return;
                }
                cardLogic.SetFocusCardLogic();
                foreach (CardLogic target in validTargets)
                {
                    if (target is MonsterLogic monster && target.dataLogic.currentLocation == Location.Field)
                        monster.currentSlot.effectTarget.SetActive(true);
                    if (target is GodLogic god && target.dataLogic.currentLocation == Location.Field)
                        god.dataLogic.cardController.heroEffectTarget.SetActive(true);
                    if (target.dataLogic.currentLocation == Location.Grave)
                        target.dataLogic.cardController.graveTarget.SetActive(true);
                    if (target.dataLogic.currentLocation == Location.Deck)
                        target.dataLogic.cardController.deckTarget.SetActive(true);
                }
                if (cardLogic.dataLogic.cardController.isAI)
                {
                    cardLogic.dataLogic.cardController.AIManager.GetEffectTarget();
                    return;
                }
                cardLogic.gameManager.EnableCardScrollScreen(validTargets, !cardLogic.effectLogic.focusSubEffect.EffectActivationIsMandatory);
                return;
            }
            switch (cardLogic.effectLogic.focusSubEffect.targetingType)
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
        if (cardLogic.gameManager.gameState != GameState.Targeting && targets.Count == 0)
            cardLogic.EffectResolution(subEffect);
    }

    public void ManualTargetAcquisition(SubEffect subEffect)
    {
        if (cardLogic.gameManager.gameState != GameState.Targeting)
            return;

        CardLogic targeter = cardLogic.gameManager.currentFocusCardLogic;
        if (!targeter.targetingLogic.validTargets.Contains(cardLogic))
            return;
        if (targeter.targetingLogic.targets == null)
            targeter.targetingLogic.targets = new();
        else if (targeter.targetingLogic.targets.Contains(cardLogic))
            return;
        targeter.targetingLogic.targets.Add(cardLogic);
        //if you hit the needed amount of targets or all valid targets are taken, resolve
        if (targeter.targetingLogic.targets.Count == targeter.effectLogic.focusSubEffect.effectTargetAmount ||
            targeter.targetingLogic.targets.Count == targeter.targetingLogic.validTargets.Count)
        {
            cardLogic.gameManager.DisableCardScrollScreen();
            targeter.EffectResolution(subEffect);
            return;
        }
    }

    public void ManualTargetRemoval()
    {
        if (cardLogic.gameManager.gameState != GameState.Targeting)
            return;
        CardLogic targeter = cardLogic.gameManager.currentFocusCardLogic;
        if (targeter.targetingLogic.targets == null)
            return;
        if (targeter.targetingLogic.targets.Contains(cardLogic))
            targeter.targetingLogic.targets.Remove(cardLogic);
    }

    public void AutoTargetAcquisition(SubEffect subEffect)
    {
        cardLogic.effectLogic.focusSubEffect = subEffect;
        //auto self target effects
        if (cardLogic.effectLogic.focusSubEffect.AllowSelfTarget == true && cardLogic.effectLogic.focusSubEffect.effectTargetAmount == 1)
            targets = new() { cardLogic };
        else
            targets = new(validTargets);
        cardLogic.EffectResolution(subEffect);
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
        cardLogic.EffectResolution(subEffect);
        return;
    }
}