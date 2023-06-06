using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayableLogic : MonoBehaviour
{
    public Game_Manager gm;
    public CardLogic logic;

    public int cost;
    public bool hasBeenPlayed, hasGottenTargets, hasDoneHoverEffect;

    private string playError;

    public void PlayCoroutineHandler(PlayerManager player) => StartCoroutine(PlayCoroutine(player));

    private IEnumerator PlayCoroutine(PlayerManager player)
    {
        transform.position = player.activationZone.position;
        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        CardPlayed(player);
        yield break;
    }

    public void EnableHover()
    {
        if (hasBeenPlayed == false && logic.currentLocation == CardLogic.Location.Hand && hasDoneHoverEffect == false && gm.gameState == Game_Manager.GameState.Open)
        {
            if (logic.cardOwner == gm.BluePlayerManager)
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
            gm.currentFocusCardLogic = logic;
            gm.StateChange(Game_Manager.GameState.Activation);
            if(!ignoreCost)
                player.costCount -= cost;
            transform.SetParent(null);
            logic.currentLocation = CardLogic.Location.Limbo;
            if (playString == "deploy")
                player.isEmptyHandSlot[logic.locationOrderNumber] = true;
            gm.ShuffleHand(player);
            logic.cardOwner.handSize--;
            if (playString == "revive")
            {
                logic.cardOwner.graveLogicList.Remove(logic);
                gm.StateChange(Game_Manager.GameState.Revive);
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
        if (hasBeenPlayed == false && logic.currentLocation == CardLogic.Location.Hand && gm.gameState != Game_Manager.GameState.Targeting)
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
                    if (j > 0 && activatingEffect.EffectUsed[j - 1] == "Target")
                        continue;
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
        gm.StateChange(Game_Manager.GameState.Playing);
        if (logic.cardType == "monster")
            gameObject.GetComponent<MonsterLogic>().MonsterSummon(player);
        logic.EffectRefresh();
        gm.StateChange(Game_Manager.GameState.Deployment);
        for (int i = 0; i < logic.effects.Count; i++)
        {
            if (logic.effects[i].EffectType.Contains("Deployment"))
            {
                int j = logic.effects[i].EffectType.FindIndex(a => a == "Deployment");
                    gm.activationChainList.Add(logic);
                    gm.activationChainNumber.Add(i);
                    gm.activationChainSubNumber.Add(j);
                    break;
            }
        }
        gm.ChainResolution();
        return;
    }

    public void MoveToGrave()
    {
        transform.position = logic.cardOwner.grave.transform.position;
        logic.currentLocation = CardLogic.Location.Grave;
        logic.cardOwner.graveLogicList.Add(logic);
        logic.locationOrderNumber = logic.cardOwner.graveLogicList.FindIndex(a=> a==logic);
        gm.StateChange(Game_Manager.GameState.Grave);
        return;
    }
}

