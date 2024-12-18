using UnityEngine;
using System.Collections.Generic;

public class ChainManager : MonoBehaviour
{
    public Game_Manager gm;

    //gets chain effects that trigger on phase change
    public void GetPhaseTriggers(Phase phase)
    {
        List<CardLogic> triggered = new(FindObjectsOfType<CardLogic>());
        foreach(CardLogic triggeredCard in triggered)
        {
            //ignore vanilla
            if (triggeredCard.effects.Count == 0)
                continue;
            foreach (Effect effect in triggeredCard.effects)
            {
                //ignore no phase triggers
                if (effect.triggerPhases == null)
                    continue;

                //must be right phase, location
                if (!effect.triggerPhases.Contains(phase))
                    continue;
                if (effect.triggerLocations!=null && !effect.triggerLocations.Contains(triggeredCard.currentLocation))
                    continue;
                //ensures effects activate at right turn
                if (effect.triggerCardOwner != Controller.Undefined)
                {
                    if ((effect.triggerCardOwner == Controller.Opponent && gm.turnPlayer == triggeredCard.cardController) ||
                         (effect.triggerCardOwner == Controller.Player && gm.turnPlayer != triggeredCard.cardController))
                        continue;
                }
                //ensures one activation of an effect per card per chain
                bool addCard = true;
                int effNum = triggeredCard.effects.FindIndex(a => a == effect);
                if (gm.activationChainList.Contains(triggeredCard))
                {
                    int[] indexes = gm.activationChainList.FindAllIndexof(triggeredCard);
                    foreach (int index in indexes)
                        if (gm.activationChainSubEffectList[index].parentEffect == effect)
                            addCard = false;
                }
                if (addCard == false)
                    continue;
                Debug.Log(triggeredCard.gameObject.name + " "+effect.SubEffects[0].effectUsed);
                gm.activationChainList.Add(triggeredCard);
                gm.activationChainSubEffectList.Add(effect.SubEffects[0]);
                break;
                //because we only need to catch one sub effect per effect with trigger, the rest resolves at chain resolution
            }
        }
    }

    //gets chain effects that trigger by card effect and additional requirements beyond game state
    public void GetEffectTriggers(SubEffect subEffect, CardLogic triggerCard)
    {
        List<CardLogic> triggered = new(FindObjectsOfType<CardLogic>());
        EffectsUsed triggerEffectType = subEffect.effectUsed;

        foreach(CardLogic triggeredCard in triggered)
        {
            //ignore vanilla
            if (triggeredCard.effects.Count == 0)
                continue;
            foreach (Effect effect in triggeredCard.effects)
            {
                //ignore no triggertypes
                if (effect.triggerEffects == null)
                    continue;
                //check if it allows self triggers
                if (!effect.allowSelfTrigger && triggeredCard == triggerCard)
                    continue;
                //must be right effect trigger type, card type is either defined and correct or undefined, info is either defined and set to card name substring or undefined, location is defined and correct, must be right card owner trigger
                if (!effect.triggerEffects.Contains(triggerEffectType))
                    continue;
                if (effect.triggerCardLocations != null && !effect.triggerCardLocations.Contains(triggeredCard.currentLocation))
                    continue;

                if (effect.triggerCardTypes != null && !effect.triggerCardTypes.Contains(triggerCard.type))
                    continue;
                if (effect.TriggerInfo != null)
                {
                    bool foundTrigger = false;
                    foreach (string info in effect.TriggerInfo)
                    {
                        if (info[..3] == "has")
                            if (!triggerCard.cardName.Contains(info[3..]))
                                continue;
                        if (info[..4] == "nhas")
                            if (triggerCard.cardName.Contains(info[4..]))
                                continue;
                        if (info[..2] == "is")
                            if (triggerCard.cardName != info[2..])
                                continue;
                        if (info[..3] == "not")
                            if (triggerCard.cardName == info[3..])
                                continue;
                        foundTrigger = true;
                    }
                    if (foundTrigger == false)
                        continue;
                }
                if (effect.triggerLocations == null)
                    continue;
                if(!effect.triggerLocations.Contains(triggeredCard.currentLocation))
                    continue;
                if (effect.triggerCardOwner != Controller.Undefined)
                {
                    if ((effect.triggerCardOwner == Controller.Opponent && triggerCard.cardController == triggeredCard.cardController) ||
                         (effect.triggerCardOwner == Controller.Player && triggerCard.cardController != triggeredCard.cardController))
                        continue;
                }

                int effNum = triggeredCard.effects.FindIndex(a => a == effect);

                //ensures one activation of an effect per card per chain
                bool addCard = true;
                if (gm.activationChainList.Contains(triggeredCard))
                {
                    int[] indexes = gm.activationChainList.FindAllIndexof<CardLogic>(triggeredCard);
                    foreach (int index in indexes)
                        if (gm.activationChainSubEffectList[index].parentEffect == effect)
                            addCard = false;
                }
                if (addCard == false)
                    continue;
                Debug.Log(triggeredCard.gameObject.name + " " + effect.SubEffects[0].effectUsed);
                gm.activationChainList.Add(triggeredCard);
                gm.activationChainSubEffectList.Add(effect.SubEffects[0]);
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
                if (triggeredEffect.triggerStates == null)
                    continue;
                //check if it allows self triggers
                if (!triggeredEffect.allowSelfTrigger && triggeredCard == cardLogic)
                    continue;
                //must be right game state, card type is either defined and correct or undefined, info is either defined and set to card
                //name substring or undefined, location is defined and correct
                if (!triggeredEffect.triggerStates.Contains(gameState))
                    continue;
                if (triggeredEffect.triggerLocations == null)
                    continue;
                if (!triggeredEffect.triggerLocations.Contains(triggeredCard.currentLocation))
                    continue;
                if (triggeredEffect.triggerCardTypes != null && !triggeredEffect.triggerCardTypes.Contains(cardLogic.type))
                    continue;
                if (triggeredEffect.triggerCardLocations != null && !triggeredEffect.triggerCardLocations.Contains(cardLogic.currentLocation))
                    continue;
                if (triggeredEffect.TriggerInfo != null)
                {
                    bool foundTrigger = false;
                    foreach (string info in triggeredEffect.TriggerInfo)
                    {
                        if (info[..3] == "has" && !cardLogic.cardName.Contains(info[3..]) || info[..4] == "nhas" && cardLogic.cardName.Contains(info[4..]))
                            continue;
                        if (info[..2] == "is" && cardLogic.cardName != info[2..] || info[..3] == "not" && cardLogic.cardName == info[3..])
                            continue;
                        foundTrigger = true; 
                    }
                    if (foundTrigger == false)
                        continue;
                }  
                
                int effNum = triggeredCard.effects.FindIndex(a => a == triggeredEffect);

                //ensures that the trigger activates due to correct owner
                if (triggeredEffect.triggerCardOwner != Controller.Undefined)
                {
                    if ((triggeredEffect.triggerCardOwner == Controller.Opponent && cardLogic.cardController == triggeredCard.cardController) ||
                         (triggeredEffect.triggerCardOwner == Controller.Player && cardLogic.cardController != triggeredCard.cardController))
                        continue;
                }

                //ensures one activation of an effect per card per chain
                bool addCard = true;
                if (gm.activationChainList.Contains(triggeredCard))
                {
                    int[] indexes = gm.activationChainList.FindAllIndexof<CardLogic>(triggeredCard);
                    foreach (int index in indexes)
                        if (gm.activationChainSubEffectList[index].parentEffect == triggeredEffect)
                            addCard = false;
                        else
                            Debug.Log(gm.activationChainSubEffectList[index].parentEffect.EffectName + " " + triggeredEffect.EffectName);
                }
                if (addCard == false)
                    continue;
                Debug.Log(triggeredCard.gameObject.name + " " + triggeredEffect.SubEffects[0].effectUsed);
                gm.activationChainList.Add(triggeredCard);
                gm.activationChainSubEffectList.Add(triggeredEffect.SubEffects[0]);
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
                if (triggeredEffect.triggerStates == null)
                    continue;

                //must be right game state, card type is undefined, info is undefined, location is defined and correct
                if (!triggeredEffect.triggerStates.Contains(gameState))
                    continue;

                if (triggeredEffect.triggerCardTypes != null && triggeredEffect.triggerCardTypes.Count > 0)
                    continue;
                if (triggeredEffect.TriggerInfo != null && triggeredEffect.TriggerInfo.Count > 0)
                    continue;
                if (!triggeredEffect.triggerLocations.Contains(triggeredCard.currentLocation))
                    continue;
                if (triggeredEffect.triggerCardOwner != Controller.Undefined)
                {
                    if ((triggeredEffect.triggerCardOwner == Controller.Opponent && gm.turnPlayer == triggeredCard.cardController) ||
                         (triggeredEffect.triggerCardOwner == Controller.Player && gm.turnPlayer != triggeredCard.cardController))
                        continue;
                }
                int effNum = triggeredCard.effects.FindIndex(a => a == triggeredEffect);


                //ensures one activation of an effect per card per chain
                bool addCard = true;
                if (gm.activationChainList.Contains(triggeredCard))
                {
                    int[] indexes = gm.activationChainList.FindAllIndexof<CardLogic>(triggeredCard);
                    foreach (int index in indexes)
                        if (gm.activationChainSubEffectList[index].parentEffect == triggeredEffect)
                            addCard = false;
                }
                if (addCard == false)
                    continue;
                Debug.Log(triggeredCard.gameObject.name+" " +triggeredEffect.SubEffects[0].effectUsed);
                gm.activationChainList.Add(triggeredCard);
                gm.activationChainSubEffectList.Add(triggeredEffect.SubEffects[0]);
                //once again, only need to catch one sub effect trigger per effect,rest resolves at chain resolution
            }
        }
    }

    //chain resolution in order of addition
    public void ChainResolution()
    {
        gm.StateChange(GameState.ChainResolution);
        //get reference to each so that they can be safely removed then activated before coroutines ruin call sequence
        CardLogic cardLogic = gm.activationChainList[0];
        SubEffect subEffect = gm.activationChainSubEffectList[0];
        gm.activationChainList.RemoveAt(0);
        gm.activationChainSubEffectList.RemoveAt(0);
        //for non ai players to decide to use optionals
        if (!subEffect.EffectActivationIsMandatory && !cardLogic.cardController.isAI)
        {
            cardLogic.focusSubEffect = subEffect;
            cardLogic.SetFocusCardLogic();
            gm.EnableActivationPanel();
            return;
        }
        //ai optionals negation check
        else if (subEffect.EffectActivationIsMandatory && cardLogic.cardController.isAI)
            if (!cardLogic.cardController.AIManager.ActivateOptionalEffect())
                gm.ChainResolution();
        //else it's mandatory or has been accepted to go forward
        cardLogic.EffectActivation(subEffect);
    }
}