using UnityEngine;
using System.Linq;

public class CardTriggerLogic : MonoBehaviour
{
    private CardLogic cardLogic;

    public void Initialize() => cardLogic = GetComponent<CardLogic>();
    public void GetPhaseTriggers(Phase phase)
    {
        if (cardLogic.effectLogic.effects.Count == 0)
            return;
        foreach (Effect effect in cardLogic.effectLogic.effects)
        {
            if (effect.triggerPhases == null)
                continue;
            if (!effect.triggerPhases.Contains(phase) || !GenericTriggerChecks(effect, null))
                continue;
            if (!AddEffectToChain(effect))
                continue;
        }
    }

    public void GetEffectTriggers(SubEffect subEffect, CardLogic triggerCard)
    {
        if (cardLogic.effectLogic.effects.Count == 0)
            return;
        EffectsUsed triggerEffectType = subEffect.effectUsed;
        foreach (Effect effect in cardLogic.effectLogic.effects)
        {
            if (effect.triggerEffects == null)
                continue;
            if (!effect.triggerEffects.Contains(triggerEffectType)
                || !GenericTriggerChecks(effect, triggerCard)
                || !SecondaryTriggerChecks(triggerCard, effect))
                continue;
            if (!AddEffectToChain(effect))
                continue;
        }
    }

    public void GetStateTriggers(GameState gameState, CardLogic triggerCardLogic)
    {
        if (cardLogic.effectLogic.effects.Count == 0)
            return;
        foreach (Effect triggeredEffect in cardLogic.effectLogic.effects)
        {
            if (triggeredEffect.triggerStates == null)
                continue;
            if (!triggeredEffect.triggerStates.Contains(gameState)
                || !GenericTriggerChecks(triggeredEffect, triggerCardLogic)
                || !SecondaryTriggerChecks(triggerCardLogic, triggeredEffect))
                continue;
            if (!AddEffectToChain(triggeredEffect))
                continue;
            return;
        }
    }

    private bool GenericTriggerChecks(Effect effect, CardLogic triggerCardLogic)
    {
        if (effect.currentActivations >= effect.maxActivations && effect.maxActivations != 0)
            return false;
        if (effect.triggerLocations != null && !effect.triggerLocations.Contains(cardLogic.dataLogic.currentLocation))
            return false;
        //ensures that the trigger activates due to correct owner or turn in case of empty triggers
        if (effect.triggerCardOwner != Controller.Undefined)
        {
            //empty triggers not invoked by card logic (turn, phase change... etc)
            if (triggerCardLogic == null)
            {
                if ((effect.triggerCardOwner == Controller.Opponent && cardLogic.gameManager.turnPlayer == cardLogic.dataLogic.cardController) ||
                     (effect.triggerCardOwner == Controller.Player && cardLogic.gameManager.turnPlayer != cardLogic.dataLogic.cardController))
                    return false;
            }
            //triggers invoked by card logic (death, status change... etc)
            else if ((effect.triggerCardOwner == Controller.Opponent && cardLogic.dataLogic.cardController == triggerCardLogic.dataLogic.cardController) ||
                 (effect.triggerCardOwner == Controller.Player && cardLogic.dataLogic.cardController != triggerCardLogic.dataLogic.cardController))
                return false;
        }
        return true;
    }

    private bool SecondaryTriggerChecks(CardLogic triggerCard, Effect effect)
    {
        if (effect.triggerCardLocations != null && !effect.triggerCardLocations.Contains(triggerCard.dataLogic.currentLocation))
            return false;
        if (!effect.allowSelfTrigger && this == triggerCard)
            return false;
        if (effect.triggerCardTypes != null && !effect.triggerCardTypes.Contains(triggerCard.dataLogic.type))
            return false;

        if (effect.TriggerInfo != null)
        {
            bool foundTrigger = false;
            foreach (string info in effect.TriggerInfo)
            {
                if ((info[..3] == "has" && !triggerCard.dataLogic.cardName.Contains(info[3..]))
                    || (info[..4] == "nhas" && triggerCard.dataLogic.cardName.Contains(info[4..]))
                    || (info[..2] == "is" && triggerCard.dataLogic.cardName != info[2..])
                    || (info[..3] == "not" && triggerCard.dataLogic.cardName == info[3..]))
                    continue;
                foundTrigger = true;
            }
            return foundTrigger;
        }
        return true;
    }

    private bool AddEffectToChain(Effect triggeredEffect)
    {
        //ensures one activation of an effect per card per chain
        bool addCard = true;
        if (cardLogic.gameManager.activationChainList.Contains(cardLogic))
            foreach (var _ in cardLogic.gameManager.activationChainList.FindAllIndexof<CardLogic>(cardLogic)
                .Where(index => cardLogic.gameManager.activationChainSubEffectList[index].parentEffect == triggeredEffect)
                .Select(index => new { }))
                addCard = false;
        if (addCard)
        {
            cardLogic.gameManager.activationChainList.Add(cardLogic);
            cardLogic.gameManager.activationChainSubEffectList.Add(triggeredEffect.SubEffects[0]);
        }
        return addCard;
        //only need to catch one sub effect trigger per effect,rest resolves at chain resolution
    }
}