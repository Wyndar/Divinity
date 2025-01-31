using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterLogic : CardLogic
{
    public GameBattleManager gm;
    public CombatantLogic combatLogic;
    public PlayableLogic playLogic;
    public CardSlot currentSlot;
    public bool hasMoved;

    public void OnFieldAtkRefresh()
    {
        TMP_Text atkText = currentSlot.atkIcon.GetComponentInChildren<TMP_Text>();
        atkText.color = combatLogic.atk != combatLogic.currentAtk ? combatLogic.currentAtk < combatLogic.atk
            ? Color.red : Color.blue : Color.black;
        atkText.text = combatLogic.currentAtk.ToString();
    }
    public void OnFieldHpRefresh()
    {
        TMP_Text hpText = currentSlot.hpIcon.GetComponentInChildren<TMP_Text>();
        hpText.color = combatLogic.currentHp != combatLogic.maxHp ? combatLogic.currentHp < combatLogic.maxHp 
            ? Color.red : Color.blue : Color.black;
        hpText.text = combatLogic.currentHp.ToString();
    }
    public void MonsterSummon(PlayerManager player)
    {
        if (gm.currentFocusCardSlot != null)
            currentSlot = gm.currentFocusCardSlot;
        else foreach (CardSlot cardSlot in player.cardSlots)
                if (cardSlot.cardInZone == null && !cardSlot.isFrontline)
                {
                    currentSlot = cardSlot;
                    break;
                }
        currentSlot.cardInZone = this;
        transform.position = new(currentSlot.transform.position.x - 0.7f, currentSlot.transform.position.y, 0);
        transform.localScale = new(6, 5);
        LocationChange(Location.Field, currentSlot.column);
        player.fieldLogicList.Add(this);
        combatLogic.currentAtk = combatLogic.atk;
        combatLogic.maxHp = combatLogic.hp;
        combatLogic.currentHp = combatLogic.hp;
        currentSlot.atkIcon.SetActive(true);
        currentSlot.hpIcon.SetActive(true);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
        visualsLogic.textCanvas.gameObject.SetActive(false);
        visualsLogic.EnergyIcon.gameObject.SetActive(false);
        audioManager.NewAudioPrefab(audioManager.summon);
        GameObject go = Instantiate(dataLogic.cardController.ui.summoningCirclePrefab, currentSlot.transform, false);
        go.transform.localPosition = Vector3.zero;
        hasMoved = true;
        combatLogic.attacksLeft = combatLogic.maxAttacks;
        gm.currentFocusCardSlot = null;
        gm.StateChange(GameState.Summon);
    }

    public void DeclareMove()
    {
        foreach (CardSlot cardSlot in dataLogic.cardController.cardSlots)
            if (Mathf.Abs(currentSlot.row - cardSlot.row) <= 1 && Mathf.Abs(currentSlot.column - cardSlot.column) <= 1)
            {
                if (cardSlot == currentSlot)
                    cardSlot.sprite.color = Color.cyan;
                else if (cardSlot.cardInZone == null)
                    cardSlot.sprite.color = Color.green;
                else
                    cardSlot.sprite.color = Color.red;
            }
        gm.StateChange(GameState.Moving);
    }

    public void Move(CardSlot cardSlot)
    {
        if (Mathf.Abs(currentSlot.row - cardSlot.row) > 1 || Mathf.Abs(currentSlot.column - cardSlot.column) > 1 ||
            cardSlot == currentSlot || cardSlot.cardInZone != null || cardSlot.controller != dataLogic.cardController)
            return;
        hasMoved = true;
        currentSlot.cardInZone = null;
        currentSlot.CleanUpIcons();
        currentSlot = cardSlot;
        cardSlot.cardInZone = this;
        transform.position = new(currentSlot.transform.position.x - 0.7f, currentSlot.transform.position.y, 0);
        LocationChange(Location.Field, currentSlot.column);
        currentSlot.atkIcon.SetActive(true);
        currentSlot.hpIcon.SetActive(true);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
        foreach(CardStatus status in combatLogic.cardStatuses)
            currentSlot.SetStatusIcon(status);
        foreach (CardSlot slot in dataLogic.cardController.cardSlots)
            slot.sprite.color = dataLogic.cardController.playerColor;
        gm.StateChange(GameState.Moved);
        gm.ChainResolution();
    }
    override public void StatAdjustment(int value, Status status)
    {
        currentSlot.SetStat(status, value);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
        DeathCheck();
    }

    public IEnumerator BounceCard()
    {
        if (dataLogic.cardOwner.handSize >= 10)
        {
            playLogic.MoveToGrave();
            yield break;
        }
        foreach(HandSlot handSlot in dataLogic.cardOwner.handSlots)
        {
            if (handSlot.cardInZone == null)
                continue;
            if (dataLogic.cardOwner.handSize >= 10)
                break;

            LeavingFieldSequence();
            transform.position = Vector3.zero;
            //implementing a battle log
            LocationChange(Location.Hand, dataLogic.cardOwner.handSize);
            transform.SetParent(handSlot.transform, false);
            //when playing with another player on same device flip face up only if you bounce on your turn...
            //might implement more to support this
            visualsLogic.Flip(!dataLogic.cardOwner.isLocal || dataLogic.cardOwner.isAI 
                || dataLogic.cardOwner != gm.turnPlayer && !dataLogic.cardOwner.enemy.isAI && dataLogic.cardOwner.enemy.isLocal);
            handSlot.cardInZone = this;
            dataLogic.cardOwner.handLogicList.Add(this);
            dataLogic.cardOwner.handSize = dataLogic.cardOwner.handLogicList.Count;
            break;
        }
        gm.ShuffleHand(dataLogic.cardOwner);
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
        audioManager.SelectCharacterDeathSFX(dataLogic.id);
        currentSlot.SetStat(Status.Death, 0);
        LeavingFieldSequence();
        foreach (Effect effect in effectLogic.effects)
            foreach (SubEffect subEffect in effect.SubEffects)
                if (subEffect.effectType == EffectTypes.Vengeance)
                {
                    gm.activationChainList.Add(this);
                    gm.activationChainSubEffectList.Add(subEffect);
                    break;
                    //only need to catch one sub effect per effect, rest resolves at chain resolution
                }
        playLogic.MoveToGrave();
        gm.StateChange(GameState.Death);
    }

    public void LeavingFieldSequence()
    {
        combatLogic.currentHp = 0;
        currentSlot.CleanUpIcons();
        combatLogic.cardStatuses.Clear();
        combatLogic.hasDoneCountdown = false;
        currentSlot.cardInZone = null;
        currentSlot.DeselectSlot();
        dataLogic.cardController.fieldLogicList.Remove(this);
    }
}