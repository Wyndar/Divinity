using UnityEngine;
using System.Collections.Generic;

public class ChainManager : MonoBehaviour
{
    public Game_Manager gm;
    public EnumManager enumConverter;

    //gets chain effects that trigger on phase change
    public void GetPhaseTriggers(Phase phase)
    {
        List<CardLogic> triggered = new(FindObjectsOfType<CardLogic>());
        foreach(CardLogic triggeredCard in triggered)
        {
            //ignore vanilla
            if (triggeredCard.effects.Count == 0)
                continue;
            foreach (Effect triggeredEffect in triggeredCard.effects)
            {
                //ignore no phase triggers
                if (triggeredEffect.triggerPhase.Count == 0)
                    continue;

                //must be right phase, location
                if (!triggeredEffect.triggerPhase.Contains(phase))
                    continue;

                int subNum = triggeredEffect.triggerPhase.FindIndex(a => a == phase);
                if (triggeredCard.currentLocation != triggeredEffect.triggerLocation[subNum])
                    continue;
                int effNum = triggeredCard.effects.FindIndex(a => a == triggeredEffect);

                //ensures effects activate at right turn
                if (triggeredEffect.triggerCardOwner != null)
                {
                    if ((triggeredEffect.triggerCardOwner[subNum] == Controller.Opponent && gm.turnPlayer == triggeredCard.cardController) ||
                         (triggeredEffect.triggerCardOwner[subNum] == Controller.Player && gm.turnPlayer != triggeredCard.cardController))
                        continue;
                }

                //ensures one activation of an effect per card per chain
                bool addCard = true;
                if (gm.activationChainList.Contains(triggeredCard))
                {
                    int[] indexes = gm.activationChainList.FindAllIndexof<CardLogic>(triggeredCard);
                    foreach (int index in indexes)
                        if (gm.activationChainNumber[index] == effNum)
                            addCard = false;
                }
                if (addCard == false)
                    continue;
                gm.activationChainList.Add(triggeredCard);
                gm.activationChainNumber.Add(effNum);
                gm.activationChainSubNumber.Add(subNum);
                break;
                //because we only need to catch one sub effect per effect with trigger, the rest resolves at chain resolution
            }
        }
    }

    //gets chain effects that trigger by card effect and additional requirements beyond game state
    public void GetEffectTriggers(int countNum, int subCount, CardLogic triggerCard)
    {
        List<CardLogic> triggered = new(FindObjectsOfType<CardLogic>());
        EffectsUsed triggerEffectType = triggerCard.effects[countNum].effectsUsed[subCount];

        foreach(CardLogic triggeredCard in triggered)
        {
            //ignore vanilla
            if (triggeredCard.effects.Count == 0)
                continue;
            foreach (Effect triggeredEffect in triggeredCard.effects)
            {
                //ignore no triggertypes
                if (triggeredEffect.triggerTypes.Count == 0)
                    continue;

                //must be right effect trigger type, card type is either defined and correct or undefined, info is either defined and set to card name substring or undefined, location is defined and correct, must be right card owner trigger
                if (!triggeredEffect.triggerTypes.Contains(triggerEffectType))
                    continue;
                if (triggeredEffect.triggerCardLocation != null && triggerCard.currentLocation != triggeredEffect.triggerCardLocation[subCount])
                    continue;
                int subNum = triggeredEffect.triggerTypes.FindIndex(a => a == triggerEffectType);
                if (triggeredEffect.TriggerCard != null && triggeredEffect.TriggerCard[subNum] != triggerCard.cardType)
                    continue;
                if (triggeredEffect.TriggerInfo != null && !triggerCard.cardName.Contains(triggeredEffect.TriggerInfo[subNum]))
                    continue;
                if (triggeredCard.currentLocation != triggeredEffect.triggerLocation[subNum])
                    continue;
                if (triggeredEffect.triggerCardOwner != null)
                {
                    if ((triggeredEffect.triggerCardOwner[subNum] == Controller.Opponent && triggerCard.cardController == triggeredCard.cardController) ||
                         (triggeredEffect.triggerCardOwner[subNum] == Controller.Player && triggerCard.cardController != triggeredCard.cardController))
                        continue;
                }

                            int effNum = triggeredCard.effects.FindIndex(a => a == triggeredEffect);

                //ensures one activation of an effect per card per chain
                bool addCard = true;
                if (gm.activationChainList.Contains(triggeredCard))
                {
                    int[] indexes = gm.activationChainList.FindAllIndexof<CardLogic>(triggeredCard);
                    foreach (int index in indexes)
                        if (gm.activationChainNumber[index] == effNum)
                            addCard = false;
                }
                if (addCard == false)
                    continue;
                gm.activationChainList.Add(triggeredCard);
                gm.activationChainNumber.Add(effNum);
                gm.activationChainSubNumber.Add(subNum);
                break;
                //once again, only need to catch one sub effect trigger per effect,rest resolves at chain resolution
            }
        }
    }

    //gets chain effects that trigger by game state caused by card logic
    public void GetStateTriggers(CardLogic cardLogic, GameState gameState)
    {
        List<CardLogic> triggers = new(FindObjectsOfType<CardLogic>());

        foreach(CardLogic triggeredCard in triggers)
        {
            //ignore vanilla
            if (triggeredCard.effects.Count == 0)
                continue;
            foreach (Effect triggeredEffect in triggeredCard.effects)
            {
                //ignore no triggerStaate
                if (triggeredEffect.triggerState.Count == 0)
                    continue;

                //must be right game state, card type is either defined and correct or undefined, info is either defined and set to card name substring or undefined, location is defined and correct
                if (!triggeredEffect.triggerState.Contains(gameState))
                    continue;

                int subNum = triggeredEffect.triggerState.FindIndex(a => a == gameState);
                if (triggeredEffect.TriggerCard != null && triggeredEffect.TriggerCard[subNum] != cardLogic.cardType)
                    continue;
                if (triggeredEffect.TriggerInfo != null && !cardLogic.cardName.Contains(triggeredEffect.TriggerInfo[subNum]))
                    continue;
                if (triggeredCard.currentLocation != triggeredEffect.triggerLocation[subNum])
                    continue;
                if (triggeredEffect.triggerCardLocation.Count > 0 && cardLogic.currentLocation != triggeredEffect.triggerCardLocation[subNum])
                    continue;
                int effNum = triggeredCard.effects.FindIndex(a => a == triggeredEffect);

                //ensures that the trigger activates due to correct owner
                if (triggeredEffect.triggerCardOwner.Count > 0)
                {
                    if ((triggeredEffect.triggerCardOwner[subNum] == Controller.Opponent && cardLogic.cardController == triggeredCard.cardController) ||
                         (triggeredEffect.triggerCardOwner[subNum] == Controller.Player && cardLogic.cardController != triggeredCard.cardController))
                        continue;
                }

                //ensures one activation of an effect per card per chain
                bool addCard = true;
                if (gm.activationChainList.Contains(triggeredCard))
                {
                    int[] indexes = gm.activationChainList.FindAllIndexof<CardLogic>(triggeredCard);
                    foreach (int index in indexes)
                        if (gm.activationChainNumber[index] == effNum)
                            addCard = false;
                }
                if (addCard == false)
                    continue;
                gm.activationChainList.Add(triggeredCard);
                gm.activationChainNumber.Add(effNum);
                gm.activationChainSubNumber.Add(subNum);
                break;
                //once again, only need to catch one sub effect trigger per effect,rest resolves at chain resolution
            }
        }
    }

    //gets chain effects that trigger by game state caused by regular effects
    public void GetEmptyStateTriggers(GameState gameState)
    {
        List<CardLogic> triggers = new(FindObjectsOfType<CardLogic>());
        foreach (CardLogic triggeredCard in triggers)
        {
            //ignore vanilla
            if (triggeredCard.effects.Count == 0)
                continue;
            foreach (Effect triggeredEffect in triggeredCard.effects)
            {
                //ignore no trigger state
                if (triggeredEffect.triggerState.Count == 0)
                    continue;

                //must be right game state, card type is undefined, info is undefined, location is defined and correct
                if (!triggeredEffect.triggerState.Contains(gameState))
                    continue;

                int subNum = triggeredEffect.triggerState.FindIndex(a => a == gameState);
                if (triggeredEffect.TriggerCard != null)
                    continue;
                if (triggeredEffect.TriggerInfo != null)
                    continue;
                if (triggeredCard.currentLocation != triggeredEffect.triggerLocation[subNum])
                    continue;
                int effNum = triggeredCard.effects.FindIndex(a => a == triggeredEffect);
                if (triggeredEffect.triggerCardOwner.Count > 0)
                {
                    if ((triggeredEffect.triggerCardOwner[subNum] == Controller.Opponent && gm.turnPlayer == triggeredCard.cardController) ||
                         (triggeredEffect.triggerCardOwner[subNum] == Controller.Player && gm.turnPlayer != triggeredCard.cardController))
                        continue;
                }

                //ensures one activation of an effect per card per chain
                bool addCard = true;
                if (gm.activationChainList.Contains(triggeredCard))
                {
                    int[] indexes = gm.activationChainList.FindAllIndexof<CardLogic>(triggeredCard);
                    foreach (int index in indexes)
                        if (gm.activationChainNumber[index] == effNum)
                            addCard = false;
                }
                if (addCard == false)
                    continue;
                gm.activationChainList.Add(triggeredCard);
                gm.activationChainNumber.Add(effNum);
                gm.activationChainSubNumber.Add(subNum);
                //once again, only need to catch one sub effect trigger per effect,rest resolves at chain resolution
            }
        }
    }

    //chain resolution in order of addition
    public void ChainResolution()
    {
        if (gm.isActivatingEffect)
            return;
        gm.StateChange(GameState.ChainResolution);
        for (int i = 0; i< gm.activationChainList.Count; i++)
        {
            //get reference to each so that they can be safely removed then activated before coroutines ruin call sequence
            CardLogic resolvingCard = gm.activationChainList[i];
            int resolvingEffectNumber = gm.activationChainNumber[i];
            int resolvingSubEffectNumber = gm.activationChainSubNumber[i];
            gm.activationChainList.RemoveAt(i);
            gm.activationChainNumber.RemoveAt(i);
            gm.activationChainSubNumber.RemoveAt(i);
            //for non ai players to decide to use optionals
            if (!resolvingCard.effects[resolvingEffectNumber].EffectActivationIsMandatory[resolvingSubEffectNumber] && !resolvingCard.cardController.isAI)
            {
                resolvingCard.effectCountNumber = resolvingEffectNumber;
                resolvingCard.subCountNumber = resolvingSubEffectNumber;
                gm.currentFocusCardLogic = resolvingCard;
                gm.EnableActivationPanel();
                break;
            }
            if (!resolvingCard.effects[resolvingEffectNumber].EffectActivationIsMandatory[resolvingSubEffectNumber] && resolvingCard.cardController.isAI)
                if (!resolvingCard.cardController.AIManager.ActivateOptionalEffect())
                    continue;
                resolvingCard.EffectActivation(resolvingEffectNumber, resolvingSubEffectNumber);
            
        }
    }
}