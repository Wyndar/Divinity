using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterLogic : CardLogic
{
    public Game_Manager gm;
    public CombatantLogic combatLogic;
    public PlayableLogic playLogic;
    public CardSlot currentSlot;

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
        textCanvas.gameObject.SetActive(false);
        EnergyIcon.gameObject.SetActive(false);
        audioManager.NewAudioPrefab(audioManager.summon);
        GameObject go = Instantiate(cardController.ui.summoningCirclePrefab, currentSlot.transform, false);
        go.transform.localPosition = Vector3.zero;

        combatLogic.attacksLeft = combatLogic.maxAttacks;
        gm.StateChange(GameState.Summon);
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
        if (cardOwner.handSize >= 10)
        {
            playLogic.MoveToGrave();
            yield break;
        }
        foreach(HandSlot handSlot in cardOwner.handSlots)
        {
            if (handSlot.cardInZone == null)
                continue;
            if (cardOwner.handSize >= 10)
                break;

            LeavingFieldSequence();
            transform.position = Vector3.zero;
            //implementing a battle log
            LocationChange(Location.Hand, cardOwner.handSize);
            transform.SetParent(handSlot.transform, false);
            //when playing with another player on same device flip face up only if you bounce on your turn...might implement more to support this
            if (cardOwner.isLocal && !cardOwner.isAI && (cardOwner == gm.turnPlayer || cardOwner.enemy.isAI || !cardOwner.enemy.isLocal))
                FlipFaceUp();
            else
                FlipFaceDown();

            handSlot.cardInZone = this;
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
        currentSlot.SetStat(Status.Death, 0);
        LeavingFieldSequence();
        foreach (Effect effect in effects)
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
        currentSlot.atkIcon.SetActive(false);
        currentSlot.hpIcon.SetActive(false);
        List<GameObject> allChildren = new();
        foreach (Transform child in currentSlot.fieldIcon.transform)
            allChildren.Add(child.gameObject);
        foreach (GameObject child in allChildren)
            Destroy(child);
        combatLogic.cardStatuses.Clear();
        combatLogic.hasDoneCountdown = false;
        currentSlot.cardInZone = null;
        cardController.fieldLogicList.Remove(this);
    }
}