﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CombatantLogic : MonoBehaviour
{
	public GameBattleManager gm;
    public CardLogic logic;

    public List<CombatantLogic> validTargets = new();

    public List<CardStatus> cardStatuses = new();
    public TargetState targetState;
    public CardStatus targetStatus;

    public int atk, hp, maxHp, currentAtk, currentHp, maxAttacks, attacksLeft;

    public bool hasAttacked, hasAttackedThisTurn, hasDoneCountdown, hasArmor, hasBarrier;

    private Coroutine currentCoroutine;

    public void TakeDamage(int damage, bool wasAttack)
    {
        bool blockDamage = false;
        if (logic is GodLogic godLogic && targetState != TargetState.Spot)
            blockDamage = godLogic.ShieldTrigger(damage, wasAttack);
        if (!blockDamage)
            currentCoroutine = StartCoroutine(DamageResolution(damage, wasAttack));
    }

    public IEnumerator DamageResolution(int damage, bool wasAttack)
    {
        AudioClip audioClip = null;
        CardStatus armor = BuffCheck(Buffs.Armor);
        CardStatus barrier = BuffCheck(Buffs.Barrier);

        if (armor != null)
        {
            damage -= armor.Amount;
            audioClip = logic.audioManager.attackResolutionArmored;
        }
        if(barrier != null)
        {
            int incomingDamage = damage;
            damage -= barrier.Amount;
            barrier.Amount -= incomingDamage;
            if (barrier.Amount <= 0)
                RemoveCardStatus(barrier);
            else
                barrier.fieldIconHolder.amountText.text = barrier.Amount.ToString();
            //audioClip ??= logic.audioManager.attackResolutionBarrier;
            //add audio for barrier later, k?
        }
        audioClip = audioClip == null ? logic.audioManager.attackResolution : audioClip;
        logic.audioManager.NewAudioPrefab(audioClip);
        gm.ClearAttackTargetImages();

        if (damage != 0)
        {
            if (targetState == TargetState.Spot && logic.dataLogic.type == Type.Fighter)
                damage *= 2;
            currentHp -= damage;
            logic.audioManager.SelectCharacterDamageSFX(logic.dataLogic.id);
            gm.StateChange(GameState.Damaged);
                logic.StatAdjustment(damage, Status.Damage);
            yield return new WaitForSeconds(1f);
        }
        StatChangeHistoryEntry statChangeLog = new(Status.Damage, damage)
        {
            logIndex = gm.gameLogHistoryEntries.Count,
            loggedCard = gm.currentFocusCardLogic,
            loggedLocation = logic.dataLogic.currentLocation
        };
        gm.gameLogHistoryEntries.Add(statChangeLog);
        if (!wasAttack)
            yield break;
        AfterDamageResolution();
        yield break;
    }

    private void AfterDamageResolution()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        foreach (Effect effect in logic.effectLogic.effects)
            foreach (SubEffect subEffect in effect.SubEffects)
                if (subEffect.effectType == EffectTypes.Counter)
                {
                    gm.activationChainList.Add(logic);
                    gm.activationChainSubEffectList.Add(subEffect);
                    break;
                    //only need to catch one sub effect per effect, rest resolves at chain resolution
                }
        logic.dataLogic.cardController.AIManager.isPerformingAction = false;
        logic.dataLogic.cardController.enemy.AIManager.isPerformingAction = false;
        gm.ChainResolution();
    }

    public void StatAdjustment(int value, Status status)
    {
        switch (status) {
            case Status.AtkGain:
                currentAtk += value;
                break;
            case Status.AtkLoss:
                currentAtk -= value;
                break;
            case Status.HpGain:
                maxHp += value;
                currentHp += value;
                break;
            case Status.HpLoss:
                maxHp -= value;
                currentHp -= value;
                break;
            default:
                throw new MissingReferenceException($"Error at stat type {status} for {logic.dataLogic.cardName}");
        }
        if (currentAtk < 0)
            currentAtk = 0;
        logic.StatAdjustment(value, status);
    }

    public void Heal(int healAmount)
    {
        currentHp += healAmount;
        if(OverhealCheck())
        {
            int overhealAmount = currentHp - maxHp;
            currentHp = maxHp;
            healAmount -= overhealAmount;
        }
        StatChangeHistoryEntry statChangeLog = new(Status.Heal, healAmount)
        {
            logIndex = gm.gameLogHistoryEntries.Count,
            loggedCard = gm.currentFocusCardLogic,
            loggedLocation = logic.dataLogic.currentLocation
        };

        gm.gameLogHistoryEntries.Add(statChangeLog);
        logic.StatAdjustment(healAmount, Status.Heal);
        if (logic is MonsterLogic monsterLogic)
            monsterLogic.OnFieldHpRefresh();
    }

    public bool OverhealCheck()
    {
        if (currentHp > maxHp)
            return true;
        return false;
    }

    public void AttackResolution()
    {
        CombatantLogic attacker = gm.currentFocusCardLogic.gameObject.GetComponent<CombatantLogic>();
        foreach (Effect effect in gm.currentFocusCardLogic.effectLogic.effects)
            foreach (SubEffect subEffect in effect.SubEffects)
                if (subEffect.effectType == EffectTypes.Assault)
                {
                    gm.activationChainList.Add(gm.currentFocusCardLogic);
                    gm.activationChainSubEffectList.Add(subEffect);
                    break;
                    //only need to catch one sub effect per effect, rest resolves at chain resolution
                }
        //bugged needs reconsideration
        //logic.U_I.DrawAttackArrow(attacker.logic, logic);
        TakeDamage(attacker.currentAtk, true);
    }

    public List<CombatantLogic> GetValidAttackTargets()
    {
        List<CombatantLogic> logics = new();

        //if provoked, just return the provoker
        if (DebuffCheck(Debuffs.Provoked) != null)
        {
            foreach (Debuff d in cardStatuses.Cast<Debuff>())
                if (d.debuff == Debuffs.Provoked)
                {
                    logics.Add(d.applierLogic.GetComponent<CombatantLogic>());
                    return logics;
                }
        }

        //else check for taunters and stealthed units then return valids
        bool tauntEnemy = false;
        bool blockerInColumn = false;

        //check hero 
        CombatantLogic hero = logic.dataLogic.cardController.enemy.heroCardLogic.GetComponent<CombatantLogic>();
        if (hero.targetState == TargetState.Taunt)
        {
            tauntEnemy = true;
            logics.Add(hero);
        }

        //then check if there is a taunter on field
        foreach (CardLogic card in logic.dataLogic.cardController.enemy.fieldLogicList)
        {
            CombatantLogic combatantLogic = card.GetComponent<CombatantLogic>();
            if (combatantLogic.targetState == TargetState.Taunt)
            {
                tauntEnemy = true;
                break;
            }
        }
        bool isFrontline = logic.GetComponent<MonsterLogic>().currentSlot.isFrontline;
        //then you can add based on taunt and stealth
        foreach (CardLogic cardLogic in logic.dataLogic.cardController.enemy.fieldLogicList)
        {
            CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
            if (tauntEnemy)
                if (combatantLogic.targetState != TargetState.Taunt)
                    continue;

            if (combatantLogic.targetState==TargetState.Stealth)
                continue;
            //make sure it's not already there
            if (!logics.Contains(combatantLogic) && (cardLogic.GetComponent<MonsterLogic>().currentSlot.isFrontline || isFrontline))
                logics.Add(combatantLogic);
            if (cardLogic.GetComponent<MonsterLogic>().currentSlot.column == logic.GetComponent<MonsterLogic>().currentSlot.column)
                blockerInColumn = true;
            if (!tauntEnemy && !logics.Contains(hero) && (logic.dataLogic.type != Type.Fighter || isFrontline) && !blockerInColumn)
                logics.Add(hero);
        }
        if (logics.Count == 0 && (logic.dataLogic.type != Type.Fighter || isFrontline) && hero.targetState != TargetState.Stealth)
            logics.Add(hero);
        validTargets = new(logics);
        return logics;
    }

    public void AttackTargetAcquisition()
    {
        if (gm.gameState != GameState.AttackDeclaration)
            return;
        CombatantLogic attacker = gm.currentFocusCardLogic.GetComponent<CombatantLogic>();
        if (attacker.validTargets.Contains(this))
            AttackResolution();
    }
    public void RemoveTargetStatus()
    {
        if (targetState != TargetState.Default)
        {
            targetStatus?.fieldIconHolder.DestroyThis();
            cardStatuses.Remove(targetStatus);
            targetStatus = null;
            targetState = TargetState.Default;
        }
    }
    public void SetTargetStatus(CardStatus status, TargetState state)
    {
        RemoveTargetStatus();
        targetStatus = status;
        targetState = state;
        cardStatuses.Add(status);
        logic.GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(status);
    }

    public void  AddNonStackingBuff(Buff buff)
    {
        CardStatus cardStatus = BuffCheck(buff.buff);
        if (cardStatus != null)
        {
            //use timers(if any) to get duration and leave current buff if it has a longer duration
            if (cardStatus.shouldCountdown)
                if (cardStatus.Timer >= buff.Timer)
                    return;
            //use amount (if>0) to leave stronger buffs
            if (buff.Amount > 0)
                if (cardStatus.Amount >= buff.Amount)
                    return;
            RemoveCardStatus(cardStatus);
        }
        cardStatuses.Add(buff);
        logic.GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(buff);
    }

    public void AddNonStackingDebuff(Debuff debuff)
    {
        CardStatus cardStatus = DebuffCheck(debuff.debuff);
        if (cardStatus != null)
        {
            //use timers(if any) to get duration and leave current buff if it has a longer duration
            if (cardStatus.shouldCountdown)
                if (cardStatus.Timer >= debuff.Timer)
                    return;
            //use amount (if>0) to leave stronger buffs
            if (debuff.Amount > 0)
                if (cardStatus.Amount >= debuff.Amount)
                    return;
            RemoveCardStatus(cardStatus);
        }
        cardStatuses.Add(debuff);
        if (logic is MonsterLogic monster)
            monster.currentSlot.SetStatusIcon(debuff);
    }

    //can safely implement for a buff/debuff cleanse effect
    public void RemoveCardStatus(CardStatus cardStatus)
    {
        if (cardStatuses.Contains(cardStatus))
        {
            if (cardStatus.fieldIconHolder != null)
                cardStatus.fieldIconHolder.DestroyThis();
            cardStatuses.Remove(cardStatus);
        }
    }
    public CardStatus DebuffCheck(Debuffs debuff)
    {
        if (cardStatuses.Count < 1)
            return null;

        //generic debuff check
        if (debuff == Debuffs.Undefined)
            foreach (CardStatus stat in cardStatuses)
                if (stat is Debuff)
                    return stat;

        foreach (CardStatus status in cardStatuses)
            if (status is Debuff d)
                if (d.debuff == debuff)
                    return d;

        return null;
    }
    public CardStatus BuffCheck(Buffs buff)
    {
        if (cardStatuses.Count < 1)
            return null;

        //generic buff check
        if(buff== Buffs.Undefined)
            foreach(CardStatus stat in cardStatuses)
                if(stat is Buff)
                    return stat;

        foreach (CardStatus status in cardStatuses)
            if (status is Buff b)
                if (b.buff == buff)
                    return b;

        return null;
    }

    public bool ImmobilityCheck() => cardStatuses.Count < 1 ? false : DebuffCheck(Debuffs.Stunned) != null 
        ? true : DebuffCheck(Debuffs.Sleeping) != null;

    public bool ValidAttackerCheck() => !ImmobilityCheck() && attacksLeft > 0 && currentAtk > 0 &&
        GetValidAttackTargets().Count != 0;

    public void DeclareAttack()
    {
        attacksLeft -= 1;
        hasAttacked = true;
        hasAttackedThisTurn = true;
        gm.StateChange(GameState.AttackDeclaration);

        foreach (CombatantLogic combatantLogic in validTargets)
        {
            if (combatantLogic.logic is MonsterLogic monster)
                monster.currentSlot.attackTarget.SetActive(true);
            if (combatantLogic.logic.dataLogic.type == Type.God)
                combatantLogic.logic.dataLogic.cardController.heroAttackTarget.SetActive(true);
        }
        logic.SetFocusCardLogic();

        if (!logic.dataLogic.cardController.isAI)
            return;
        CombatantLogic godLogic = validTargets.Find(x => x.logic is GodLogic);
        if (godLogic != null)
            godLogic.AttackTargetAcquisition();
        else
        {
            IOrderedEnumerable<CombatantLogic> sortedTargets = validTargets.OrderBy(x => x.currentAtk);
            List<CombatantLogic> targets = new(sortedTargets);
            targets[^1].AttackTargetAcquisition();
        }
    }

    //to avoid changed list errors
    public void TurnTimer()
    {
        bool brokeLoop = false;
        if (cardStatuses.Count > 0)
            foreach (CardStatus cardStatus in cardStatuses)
                if (cardStatus.hasDoneCountDownThisTurn)
                    continue;
                else
                {
                    brokeLoop = true;
                    gm.StatusCountdown(cardStatus);
                    break;
                }
        if (!brokeLoop)
            hasDoneCountdown = true;
        gm.AllTimersCountdown();
    }

    public void ResetCountdown()
    {
        if (cardStatuses.Count > 0)
            foreach (CardStatus cardStatus in cardStatuses)
                gm.StatusCountdownReset(cardStatus);
        hasDoneCountdown = false;
    }

    public void AttackRefresh() => attacksLeft = maxAttacks;
}