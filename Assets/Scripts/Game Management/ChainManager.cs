using UnityEngine;
using System.Collections.Generic;

public class ChainManager : MonoBehaviour
{
    public Game_Manager gm;
    public EnumConverter enumConverter;

    //gets chain effects that trigger on phase change
    public void GetPhaseTriggers(Game_Manager.Phase phase)
    {
        List<CardLogic> triggered = new List<CardLogic>(FindObjectsOfType<CardLogic>());
        for (int j = triggered.Count - 1; j > -1; j--)
        {
            for (int k = 0; k < triggered[j].effects.Count; k++)
            {
                Effect triggerEffect = triggered[j].effects[k];
                if (triggerEffect.TriggerPhase == null)
                    continue;
                for (int l = 0; l < triggerEffect.TriggerPhase.Count; l++)
                {
                    //must be right phase, location is either defined and correct or undefined
                    if (phase != enumConverter.PhaseStringToEnum(triggerEffect.TriggerPhase[l]))
                        continue;
                    if (triggerEffect.TriggerLocation == null)
                        continue;
                    if (triggered[j].currentLocation != enumConverter.LocationStringToEnum(triggerEffect.TriggerLocation[l]) && enumConverter.LocationStringToEnum(triggerEffect.TriggerLocation[l]) != CardLogic.Location.Undefined)
                        continue;
                    //ensures one activation of an effect per card per chain
                    if (gm.activationChainList.Contains(triggered[j]))
                        if (k == gm.activationChainNumber[gm.activationChainList.FindIndex(a => a == triggered[j])])
                            continue;
                    gm.activationChainList.Add(triggered[j]);
                    gm.activationChainNumber.Add(k);
                    gm.activationChainSubNumber.Add(l);
                    break;
                    //because we only need to catch one sub effect per effect with trigger, the rest resolves at chain resolution
                }
            }
        }
    }

    //gets chain effects that trigger by card effect and additional requirements beyond game state
    public void GetEffectTriggers(int countNum, int subCount, CardLogic cardLogic)
    {
        List<CardLogic> triggered = new List<CardLogic>(FindObjectsOfType<CardLogic>());
        for (int j = triggered.Count - 1; j > -1; j--)
        {
            for (int k = 0; k < triggered[j].effects.Count; k++)
            {
                Effect triggerEffect = triggered[j].effects[k];
                Effect trigger = cardLogic.effects[countNum];
                {
                    if (triggerEffect.TriggerType == null)
                        continue;
                    for (int l = 0; l < triggerEffect.TriggerType.Count; l++)
                    {
                        //must be right effect trigger type, card type is either defined and correct or undefined, info is either defined and set to card name substring or undefined, location is either defined and correct or undefined
                        if (trigger.EffectType[subCount] != triggerEffect.TriggerType[l])
                            continue;
                        if (triggerEffect.TriggerCard != null && triggerEffect.TriggerCard[l] != cardLogic.cardType)
                            continue;
                        if (triggerEffect.TriggerInfo != null && !cardLogic.cardName.Contains(triggerEffect.TriggerInfo[l]))
                            continue;
                        if (triggerEffect.TriggerLocation != null && triggered[j].currentLocation != enumConverter.LocationStringToEnum(triggerEffect.TriggerLocation[countNum]))
                            continue;
                        //ensures one activation of an effect per card per chain
                        if (gm.activationChainList.Contains(triggered[j]))
                            if (k == gm.activationChainNumber[gm.activationChainList.FindIndex(a => a == triggered[j])])
                                continue;
                        gm.activationChainList.Add(triggered[j]);
                        gm.activationChainNumber.Add(k);
                        gm.activationChainSubNumber.Add(l);
                        break;
                        //once again, only need to catch one sub effect trigger per effect,rest resolves at chain resolution
                    }
                }   
            }
        }
    }

    //gets chain effects that trigger by game state caused by card logic
    public void GetStateTriggers(CardLogic cardLogic)
    {
        List<CardLogic> triggers = new List<CardLogic>(FindObjectsOfType<CardLogic>());

        for (int j = triggers.Count - 1; j > -1; j--)
        {
            for (int k = 0; k < triggers[j].effects.Count; k++)
            {
                Effect triggerEffect = triggers[j].effects[k];
                if (triggerEffect.TriggerState == null)
                    continue;
                for (int l = 0; l < triggerEffect.TriggerState.Count; l++)
                {
                    //must be right game state, card type is either defined and correct or undefined, info is either defined and set to card name substring or undefined, location is either defined and correct or undefined
                    if (gm.gameState != enumConverter.StateStringToEnum(triggerEffect.TriggerState[l]))
                        continue;
                    if (triggerEffect.TriggerCard != null && triggerEffect.TriggerCard[l] != cardLogic.cardType)
                        continue;
                    if (triggerEffect.TriggerInfo != null && cardLogic.cardName.Contains(triggerEffect.TriggerInfo[l]) == false)
                            continue;
                    if (triggerEffect.TriggerLocation != null && triggers[j].currentLocation == enumConverter.LocationStringToEnum(triggerEffect.TriggerLocation[l]))
                        continue;
                    //ensures one activation of an effect per card per chain
                    if (gm.activationChainList.Contains(triggers[j]))
                        if (k == gm.activationChainNumber[gm.activationChainList.FindIndex(a => a == triggers[j])])
                            continue;
                    gm.activationChainList.Add(triggers[j]);
                    gm.activationChainNumber.Add(k);
                    gm.activationChainSubNumber.Add(l);
                    break;
                    //once again, only need to catch one sub effect trigger per effect,rest resolves at chain resolution
                }
            }
        }
    }

    //gets chain effects that trigger by game state caused by regular effects
    public void GetEmptyStateTriggers()
    {
        List<CardLogic> triggers = new List<CardLogic>(FindObjectsOfType<CardLogic>());
        for (int j = 0; j < triggers.Count; j++)
        {
            for (int k = 0; k < triggers[j].effects.Count; k++)
            {
                Effect triggerEffect = triggers[j].effects[k];
                if (triggerEffect.TriggerState == null)
                    continue;
                for (int l = 0; l < triggerEffect.TriggerState.Count; l++)
                {
                    //must be right game state, card type is undefined, info is undefined, location is either defined and correct or undefined
                    if (gm.gameState != enumConverter.StateStringToEnum(triggerEffect.TriggerState[l]))
                        continue;
                    if (triggerEffect.TriggerCard != null)
                        continue;
                    if (triggerEffect.TriggerInfo != null)
                        continue;
                    if (triggerEffect.TriggerLocation != null && triggers[j].currentLocation != enumConverter.LocationStringToEnum(triggerEffect.TriggerLocation[l]))
                        continue;
                    //ensures one activation of an effect per card per chain
                    if (gm.activationChainList.Contains(triggers[j]))
                        if (k == gm.activationChainNumber[gm.activationChainList.FindIndex(a => a == triggers[j])])
                            continue;
                    gm.activationChainList.Add(triggers[j]);
                    gm.activationChainNumber.Add(k);
                    gm.activationChainSubNumber.Add(l);
                    //once again, only need to catch one sub effect trigger per effect,rest resolves at chain resolution
                }
            }
        }
    }

    //chain resolution backwards
    //!!???? why backwards?????
    //changing to in order of addition
    public void ChainResolution()
    {
        gm.StateChange(Game_Manager.GameState.ChainResolution);
        for (int i = 0; i< gm.activationChainList.Count; i++)
        {
            CardLogic resolvingCard = gm.activationChainList[i];
            int resolvingEffectNumber = gm.activationChainNumber[i];
            int resolvingSubEffectNumber = gm.activationChainSubNumber[i];
            gm.activationChainList.RemoveAt(i);
            gm.activationChainNumber.RemoveAt(i);
            gm.activationChainSubNumber.RemoveAt(i);
            if (resolvingCard.effects[resolvingEffectNumber].EffectActivationIsMandatory[resolvingSubEffectNumber] == false)
            {
                resolvingCard.effectCountNumber = resolvingEffectNumber;
                resolvingCard.subCountNumber = resolvingSubEffectNumber;
                gm.currentFocusCardLogic = resolvingCard;
                gm.EnableActivationPanel();
                break;
            }
            resolvingCard.EffectActivation(resolvingEffectNumber, resolvingSubEffectNumber);
            
        }
    }
}