﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class PlayableLogic : MonoBehaviour
{
    public GameBattleManager gm;
    public CardLogic logic;

    public int cost;
    public bool hasBeenPlayed, hasGottenTargets;
    public bool isForcedTuning = false;

    public float movementSpeed = 30f;

    private string playError;

    public void PlayCoroutineHandler(PlayerManager player) => StartCoroutine(PlayCoroutine(player));

    private IEnumerator PlayCoroutine(PlayerManager player)
    {
        float distance = Vector3.Distance(transform.position, player.activationZone.position);
        Vector3 originalPosition = transform.position;
        Vector3 direction = (player.activationZone.position - transform.position).normalized;
        float distanceTravelled = 0;
        while (distanceTravelled < distance)
        {
            Vector3 translationDistance = (player.activationZone.position - transform.position);
            if (translationDistance.magnitude > direction.magnitude)
                transform.Translate(movementSpeed * Time.deltaTime * direction, Space.World);
            else
                transform.position = player.activationZone.position;
            distanceTravelled = Vector3.Distance(originalPosition, transform.position);
            yield return null;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        AudioSource audio = logic.audioManager.NewAudioPrefab(logic.audioManager.playCard);
        yield return new WaitUntil(() => audio == null);

        transform.localScale = originalScale;
        CardPlayed(player);
        yield break;
    }

    //borrowing effects used even if all plays aren't effects to avoid redundancies in enums... should cause no issues
    public void PlayCard(EffectsUsed effectsUsed, PlayerManager player)
    {
        bool ignoreCost = false;
        bool deploy = false;
        if (effectsUsed == EffectsUsed.FreeRevive || effectsUsed == EffectsUsed.FreeDeploy)
            ignoreCost = true;
        if (effectsUsed == EffectsUsed.Deploy || effectsUsed == EffectsUsed.FreeDeploy)
            deploy = true;
        playError = LegalPlayCheck(ignoreCost, player);
        if (playError == null)
        {
            gm.isPlayingCard = true;
            if (logic.dataLogic.cardController != player)
                logic.dataLogic.ControllerSwap(player);
            logic.visualsLogic.Flip(false);
            logic.visualsLogic.GreyScaleEffect(false);
            logic.SetFocusCardLogic();
            gm.StateChange(GameState.Activation);
            if (!ignoreCost)
            {
                List<Attunement> attunements = new(logic.dataLogic.attunements);
                if (!isForcedTuning)
                    attunements.Add(Attunement.Untuned);
                player.BloodLoss(attunements, cost);
            }
            transform.SetParent(null);

                //not a real location to be logged
            logic.dataLogic.currentLocation = Location.Limbo;

            if (deploy)
            {
                player.handLogicList.Remove(logic);
                foreach (HandSlot slot in player.handSlots)
                    if (slot.cardInZone == logic)
                    {
                        slot.cardInZone = null;
                        break;
                    }
                gm.ShuffleHand(player);
            }
            else
            {
                logic.dataLogic.cardOwner.graveLogicList.Remove(logic);
                logic.dataLogic.cardOwner.underworldManager.ResetTopCard();
                gm.StateChange(GameState.Revive);
            }
            hasBeenPlayed = true;
            PlayCoroutineHandler(player);
            //the coroutine will call cardPlayed, don't worry
        }
        else
            gm.ErrorCodePanel($"You cannot play '{logic.dataLogic.cardName}' because {playError}.");
    }

    public string LegalPlayCheck(bool ignoreCost, PlayerManager player)
    {
        if (cost > player.costCount && !ignoreCost)
            return "you do not have enough blood";
        if (!ignoreCost && player.BloodAttunementCheck(Attunement.Untuned) != player.costCount)
        {
            int tempCost = cost;
            foreach (Blood b in player.bloods)
            {
                foreach (Attunement attunement in logic.dataLogic.attunements)
                    if (b.attunement == attunement)
                        tempCost--;
            }
            if (player.BloodAttunementCheck(Attunement.Untuned) < tempCost)
                return "you have not met the attunement requirements";
        }
        switch (logic.dataLogic.type)
        {
            case Type.Fighter:
                {
                    if (gm.currentFocusCardSlot != null)
                    {
                        if (gm.currentFocusCardSlot.controller != logic.dataLogic.cardController)
                            return "you do not control that zone";
                        if (gm.currentFocusCardSlot.isFrontline)
                            return "you cannot deploy to the Frontline directly";
                        if (gm.currentFocusCardSlot.cardInZone != null)
                            return "there is another card in the zone";
                        return null;
                    }
                    else foreach (CardSlot cardSlot in player.cardSlots)
                            if (cardSlot.cardInZone == null && !cardSlot.isFrontline)
                                return null;
                    return "there is no available zone";
                }

            case Type.Spell:
                {
                    foreach (SubEffect subEffect in logic.effectLogic.effects.SelectMany(effect => effect.SubEffects))
                    {
                        if (subEffect.effectUsed == EffectsUsed.BloodCost &&
                        logic.dataLogic.cardController.BloodAttunementCheck(Enum.Parse<Attunement>(subEffect.TargetStats[0])) < subEffect.effectAmount)
                            return "you cannot pay the blood cost";
                        if (subEffect.effectType != EffectTypes.Deployment || !subEffect.EffectActivationIsMandatory ||
                            subEffect.effectTargetAmount == 0 || subEffect.effectTargetAmount >= 98)
                            continue;
                        List<CardLogic> allTargetsList = logic.targetingLogic.GetValidTargets(subEffect, false);
                        if (allTargetsList.Count == 0)
                            return "there are no valid targets";
                    }
                    break;
                }
        }
        return null;
    }

    public void CardPlayed(PlayerManager player)
    {
        gm.StateChange(GameState.Playing);
        gm.StateChange(GameState.Deployment);
        if (logic.dataLogic.type == Type.Fighter)
            GetComponent<MonsterLogic>().MonsterSummon(player);
        logic.effectLogic.EffectRefresh();
        logic.SetFocusCardLogic();
        int effectsAddedCount = 0;
        foreach (Effect effect in logic.effectLogic.effects)
        {
            //passives should resolve as soon as possible, before chain stack if necessary and in order of effect displayed on card
            if (effect.SubEffects[0].effectType == EffectTypes.WhileDeployed)
            {
                gm.activationChainList.Insert(effectsAddedCount, logic);
                gm.activationChainSubEffectList.Insert(effectsAddedCount, effect.SubEffects[0]);
                effectsAddedCount++;
                continue;
                //only need to catch one, rest resolves via subsequent effect chain if any
            }
            //deployment effects should resolve before chain stack and in order of effect displayed on card
            if (effect.SubEffects[0].effectType == EffectTypes.Deployment)
            {
                gm.activationChainList.Insert(effectsAddedCount, logic);
                gm.activationChainSubEffectList.Insert(effectsAddedCount, effect.SubEffects[0]);
                effectsAddedCount++;
                //only need to catch one, rest resolves via subsequent effect chain if any
            }
        }
        AttunementPenalty(player);
        if (logic.dataLogic.cardController.isAI)
            logic.dataLogic.cardController.AIManager.isPerformingAction = false;
        gm.isPlayingCard= false;
        gm.ChainResolution();
        return;
    }

    public void MoveToGrave()
    { 
        logic.dataLogic.ControllerSwap(logic.dataLogic.cardOwner);
        transform.position = logic.dataLogic.cardOwner.shield.transform.position;
        logic.dataLogic.cardOwner.graveLogicList.Add(logic);
        logic.dataLogic.cardOwner.underworldManager.ResetTopCard();
        int i = logic.dataLogic.cardOwner.graveLogicList.FindIndex(a => a == logic);
        logic.dataLogic.LocationChange(Location.Grave, i);
        gm.StateChange(GameState.Grave);
        return;
    }
    private void AttunementPenalty(PlayerManager player)
    {
        foreach(Attunement attunement in logic.dataLogic.attunements)
        {
            if (player.heroCardLogic.dataLogic.attunements.Contains(attunement))
                continue;
            player.heroCardLogic.StatAdjustment(cost, Status.HpLoss);
        }    
    }
}

