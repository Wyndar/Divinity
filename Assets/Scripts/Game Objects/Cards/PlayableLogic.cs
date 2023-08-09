using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayableLogic : MonoBehaviour
{
    public Game_Manager gm;
    public CardLogic logic;

    public int cost;
    public bool hasBeenPlayed, hasGottenTargets, hasDoneHoverEffect;

    public float movementSpeed = 3f;

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
            if (translationDistance.magnitude <= direction.magnitude)
                transform.position = player.activationZone.position;
            else
                transform.Translate(direction * movementSpeed, Space.World);
            distanceTravelled = Vector3.Distance(originalPosition, transform.position);
            yield return null;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        CardPlayed(player);
        yield break;
    }

    public void EnableHover()
    {
        if (hasBeenPlayed == false && logic.currentLocation ==Location.Hand && hasDoneHoverEffect == false && gm.gameState == GameState.Open)
        {
            if (logic.cardController == gm.BluePlayerManager)
                transform.position += Vector3.up * 3;
            else
                transform.position -= Vector3.up * 3;
            hasDoneHoverEffect = true;
        }
    }

    public void PlayCard(string playString, bool ignoreCost, PlayerManager player)
    {
        playError = LegalPlayCheck(ignoreCost, player);
        if (playError == null)
        {
            if (logic.cardController != player)
                logic.ControllerSwap(player);
            if (logic.isFaceDown)
                logic.FlipFaceUp();
            if (!logic.isNormalColour)
                logic.NormalColour();
            gm.currentFocusCardLogic = logic;
            gm.StateChange(GameState.Activation);
            if(!ignoreCost)
                player.costCount -= cost;
            transform.SetParent(null);

                //not a real location to be logged
            logic.currentLocation = Location.Limbo;

            if (playString == "deploy")
            {
                player.handLogicList.Remove(logic);                player.isEmptyHandSlot[logic.locationOrderNumber] = true;
                gm.ShuffleHand(player);
            }
            if (playString == "revive")
            {
                logic.cardOwner.graveLogicList.Remove(logic);
                gm.StateChange(GameState.Revive);
            }
            hasBeenPlayed = true;
            PlayCoroutineHandler(player);
            //the coroutine will call cardPlayed, don't worry
        }
        else
            Debug.Log("Cannot Play " + logic.cardName + " legally because of " + playError + ".");
    }

    public void DisableHover()
    {
        if (hasBeenPlayed == false && logic.currentLocation == Location.Hand && gm.gameState != GameState.Targeting)
        {
            hasDoneHoverEffect = false;
            transform.localPosition = Vector3.zero;
        }
    }

    public string LegalPlayCheck(bool ignoreCost, PlayerManager player)
    {
        if (cost > player.costCount && !ignoreCost)
            return "Insufficient blood";
        if (logic.cardType == "monster")
        {
            for (int i = 0; i < player.isEmptyCardSlot.Length; i++)
            {
                if (player.isEmptyCardSlot[i] == true)
                    return null;
            }
            return "No fighter zones";
        }
        if (logic.cardType == "spell")
        {
            for (int i = 0; i < logic.effects.Count; i++)
            {
                
                for (int j = 0; j < logic.effects[i].EffectUsed.Count; j++)
                {
                    Effect activatingEffect = logic.effects[i];
                        if (activatingEffect.EffectType[j] != "Deployment" || activatingEffect.EffectActivationIsMandatory[j] == false)
                        continue;
                    if (activatingEffect.EffectTargetAmount == null)
                        continue;
                    if (activatingEffect.EffectTargetAmount[j] == 0)
                        continue;
                    List<CardLogic> allTargetsList = logic.GetValidTargets(i, j);
                    if (allTargetsList.Count == 0)
                        return "No valid targets";
                }
            }
        }
        return null;
    }

    public void CardPlayed(PlayerManager player)
    {
        gm.StateChange(GameState.Playing);
        if (logic.cardType == "monster")
            gameObject.GetComponent<MonsterLogic>().MonsterSummon(player);
        logic.EffectRefresh();
        gm.currentFocusCardLogic = logic;
        gm.StateChange(GameState.Deployment);
        for (int i = 0; i < logic.effects.Count; i++)
        {
            if (logic.effects[i].EffectType.Contains("Deployment"))
            {
                int j = logic.effects[i].EffectType.FindIndex(a => a == "Deployment");
                gm.activationChainList.Insert(0, logic);
                gm.activationChainNumber.Insert(0, i);
                gm.activationChainSubNumber.Insert(0, j);
                break;
            }
        }
        for (int i = 0; i < logic.effects.Count; i++)
        { 
            if (logic.effects[i].EffectType.Contains("While Deployed"))
            {
                int j = logic.effects[i].EffectType.FindIndex(a => a == "While Deployed");
                gm.activationChainList.Insert(0, logic);
                gm.activationChainNumber.Insert(0, i);
                gm.activationChainSubNumber.Insert(0, j);
                continue;
            }
        }
        if (logic.cardController.isAI)
            logic.cardController.AIManager.isPerformingAction = false;
        gm.ChainResolution();
        return;
    }

    public void MoveToGrave()
    {
        logic.ControllerSwap(logic.cardOwner);
        transform.position = logic.cardOwner.grave.transform.position;
        logic.cardOwner.graveLogicList.Add(logic);
        int i = logic.cardOwner.graveLogicList.FindIndex(a => a == logic);
        if (gm.isActivatingEffect)
            logic.LocationChange(gm.currentFocusCardLogic.focusEffect, gm.currentFocusCardLogic.focusEffect.effectsUsed[gm.currentFocusCardLogic.subCountNumber], Location.Grave, i);
        else
            logic.LocationChange(null, EffectsUsed.Undefined, Location.Grave, i);
        
        gm.StateChange(GameState.Grave);
        return;
    }
}

