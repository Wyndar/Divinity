using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class MonsterLogic : CardLogic
{
    public Game_Manager gm;
    public CombatantLogic combatLogic;
    public PlayableLogic playLogic;

    public void OnFieldAtkRefresh()
    {
        TMP_Text atkText = cardController.atkIcons[locationOrderNumber].GetComponentInChildren<TMP_Text>();
        atkText.color = combatLogic.atk != combatLogic.currentAtk ? combatLogic.currentAtk < combatLogic.atk
            ? Color.red : Color.blue : Color.black;
        atkText.text = combatLogic.currentAtk.ToString();
    }
    public void OnFieldHpRefresh()
    {
        TMP_Text hpText = cardController.hpIcons[locationOrderNumber].GetComponentInChildren<TMP_Text>();
        hpText.color = combatLogic.currentHp != combatLogic.maxHp ? combatLogic.currentHp < combatLogic.maxHp 
            ? Color.red : Color.blue : Color.black;
        hpText.text = combatLogic.currentHp.ToString();
    }
    public void MonsterSummon(PlayerManager player)
    {
        for (int slotNumber = 0; slotNumber < player.isEmptyCardSlot.Length; slotNumber++)
        {
            if (!player.isEmptyCardSlot[slotNumber])
                continue;
            transform.position = player.cardSlots[slotNumber].transform.position;
            player.isEmptyCardSlot[slotNumber] = false;
            LocationChange(Location.Field, slotNumber);
            player.fieldLogicList.Add(this);
            combatLogic.currentAtk = combatLogic.atk;
            combatLogic.maxHp = combatLogic.hp;
            combatLogic.currentHp = combatLogic.hp;
            player.atkIcons[locationOrderNumber].SetActive(true);
            player.hpIcons[locationOrderNumber].SetActive(true);
            OnFieldAtkRefresh();
            OnFieldHpRefresh();
            audioManager.NewAudioPrefab(audioManager.summon);
            GameObject go = Instantiate(cardController.ui.summoningCirclePrefab, cardController.atkIcons[locationOrderNumber].transform);
            go.transform.position = cardController.cardSlots[locationOrderNumber].transform.position;
            break;
        }

        combatLogic.attacksLeft = combatLogic.maxAttacks;
        gm.StateChange(GameState.Summon);
    }

    override public void StatAdjustment(int value, Status status)
    {
        cardController.SetStat(locationOrderNumber, status, value);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
        DeathCheck();
    }

    public IEnumerator BounceCard()
    {
        if (cardOwner.handSize >= 10)
        {
            playLogic.MoveToGrave();
            yield break;
        }
        for (int i = 0; i < cardOwner.isEmptyHandSlot.Length; i++)
        {
            if (cardOwner.isEmptyHandSlot[i] == false)
                continue;
            if (cardOwner.handSize >= 10)
                break;

            LeavingFieldSequence();
            transform.position = Vector3.zero;
            //implementing a battle log
            LocationChange(Location.Hand, i);
            transform.SetParent(cardOwner.handSlots[i].transform, false);
            //when playing with another player on same device flip face up only if you bounce on your turn...might implement more to support this
            if (cardOwner.isLocal && !cardOwner.isAI && (cardOwner == gm.turnPlayer || cardOwner.enemy.isAI || !cardOwner.enemy.isLocal))
                FlipFaceUp();
            else
                FlipFaceDown();

            cardOwner.isEmptyHandSlot[i] = false;
            cardOwner.handLogicList.Add(this);
            cardOwner.handSize = cardOwner.handLogicList.Count;
            break;
        }

        gm.ShuffleHand(cardOwner);
        gm.StateChange(GameState.Bounce);
        yield break;
    }

    public void DeathCheck()
    {
        if (combatLogic.currentHp > 0)
            return;
        MonsterDeath();
    }

    public void MonsterDeath()
    {
        audioManager.SelectCharacterDeathSFX(id);
        LeavingFieldSequence();
        cardController.SetStat(locationOrderNumber, Status.Death, 0);
        playLogic.MoveToGrave();
        gm.StateChange(GameState.Death);
    }

    public void LeavingFieldSequence()
    {
        combatLogic.currentHp = 0;
        cardController.atkIcons[locationOrderNumber].SetActive(false);
        cardController.hpIcons[locationOrderNumber].SetActive(false);
        List<GameObject> allChildren = new();
        foreach (Transform child in cardController.fieldIcons[locationOrderNumber].transform)
            allChildren.Add(child.gameObject);
        foreach (GameObject child in allChildren)
            Destroy(child);
        combatLogic.cardStatuses.Clear();
        combatLogic.hasDoneCountdown = false;
        cardController.isEmptyCardSlot[locationOrderNumber] = true;
        cardController.fieldLogicList.Remove(this);
    }
}