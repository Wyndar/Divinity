using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayableLogic : MonoBehaviour
{
    public Game_Manager gm;
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
            if (logic.cardController != player)
                logic.ControllerSwap(player);
            if (logic.isFaceDown)
                logic.FlipFaceUp();
            if (!logic.isNormalColour)
                logic.NormalColour();
            logic.SetFocusCardLogic();
            gm.StateChange(GameState.Activation);
            if (!ignoreCost)
            {
                List<Attunement> attunements = new(logic.attunements);
                if (!isForcedTuning)
                    attunements.Add(Attunement.Untuned);
                player.BloodLoss(attunements, cost);
            }
            transform.SetParent(null);

                //not a real location to be logged
            logic.currentLocation = Location.Limbo;

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
                logic.cardOwner.graveLogicList.Remove(logic);
                logic.cardOwner.underworldManager.ResetTopCard();
                gm.StateChange(GameState.Revive);
            }
            hasBeenPlayed = true;
            PlayCoroutineHandler(player);
            //the coroutine will call cardPlayed, don't worry
        }
        else
            Debug.Log($"Cannot Play {logic.cardName} legally because of {playError}.");
    }

    public string LegalPlayCheck(bool ignoreCost, PlayerManager player)
    {
        if (cost > player.costCount && !ignoreCost)
            return "Insufficient blood";
        if (!ignoreCost && player.BloodAttunementCheck(Attunement.Untuned) != player.costCount)
        {
            int tempCost = cost;
            foreach (Blood b in player.bloods)
            {
                foreach (Attunement attunement in logic.attunements)
                    if (b.attunement == attunement)
                        tempCost--;
            }
            if (player.BloodAttunementCheck(Attunement.Untuned) < tempCost)
                return "Blood Requirements not met!";
        }
        switch (logic.type)
        {
            case Type.Fighter:
                {
                    if (gm.currentFocusCardSlot != null)
                    {
                        if (gm.currentFocusCardSlot.controller != logic.cardController)
                            return "Zone is not under your control";
                        if (gm.currentFocusCardSlot.isFrontline)
                            return "Cannot deploy to Frontline directly";
                        if (gm.currentFocusCardSlot.cardInZone != null)
                            return "Occupied Zone";
                        return null;
                    }
                    else foreach (CardSlot cardSlot in player.cardSlots)
                            if (cardSlot.cardInZone == null && !cardSlot.isFrontline)
                                return null;
                    return "No fighter Zones";
                }

            case Type.Spell:
                {
                    foreach (Effect effect in logic.effects)
                    {
                        foreach (SubEffect subEffect in effect.SubEffects)
                        {
                            if (subEffect.effectType != EffectTypes.Deployment || subEffect.EffectActivationIsMandatory == false)
                                continue;
                            //need to keep an eye on this
                            if (subEffect.effectTargetAmount == 0 || subEffect.effectTargetAmount >= 98)
                                continue;
                            List<CardLogic> allTargetsList = logic.GetValidTargets(subEffect);
                            if (allTargetsList.Count == 0)
                                return "No valid targets";
                        }
                    }
                    break;
                }
        }
        return null;
    }

    public void CardPlayed(PlayerManager player)
    {
        gm.StateChange(GameState.Playing);
        if (logic.type == Type.Fighter)
            GetComponent<MonsterLogic>().MonsterSummon(player);
        logic.EffectRefresh();
        logic.SetFocusCardLogic();
        foreach (Effect effect in logic.effects)
        {
            foreach (SubEffect subEffect in effect.SubEffects)
            //deployment effects should resolve after chain stack
            if (subEffect.effectType == EffectTypes.Deployment)
            {
                gm.activationChainList.Add(logic);
                gm.activationChainSubEffectList.Add(subEffect);
                break;
                //only need to catch one, rest resolves via subsequent effect chain if any
            }
            foreach (SubEffect subEffect in effect.SubEffects)
                //passives should resolve as soon as possible, before chain stack if necessary...
                //unfortunately this causes them to resolve in reverse the order written on the card
                // in case of issues later, can reverse the loop
                if (subEffect.effectType == EffectTypes.WhileDeployed)
                {
                    gm.activationChainList.Add(logic);
                    gm.activationChainSubEffectList.Add(subEffect);
                    break;
                    //only need to catch one, rest resolves via subsequent effect chain if any
                }
        }
        AttunementPenalty(player);
        gm.StateChange(GameState.Deployment);
        if (logic.cardController.isAI)
            logic.cardController.AIManager.isPerformingAction = false;
        gm.isPlayingCard= false;
        gm.ChainResolution();
        return;
    }

    public void MoveToGrave()
    {
        logic.ControllerSwap(logic.cardOwner);
        transform.position = logic.cardOwner.shield.transform.position;
        logic.cardOwner.graveLogicList.Add(logic);
        logic.cardOwner.underworldManager.ResetTopCard();
        int i = logic.cardOwner.graveLogicList.FindIndex(a => a == logic);
        logic.LocationChange(Location.Grave, i);
        gm.StateChange(GameState.Grave);
        return;
    }
    private void AttunementPenalty(PlayerManager player)
    {
        foreach(Attunement attunement in logic.attunements)
        {
            if (player.heroCardLogic.attunements.Contains(attunement))
                continue;
            player.heroCardLogic.combatantLogic.currentHp -= cost;
            player.heroCardLogic.combatantLogic.maxHp -= cost;
        }    
    }
}

