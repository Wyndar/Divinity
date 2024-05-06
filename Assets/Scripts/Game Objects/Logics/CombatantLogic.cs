using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CombatantLogic : MonoBehaviour
{
	public Game_Manager gm;
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
        if (logic.type == Type.God)
            if (targetState != TargetState.Spot)
                blockDamage = GetComponent<GodLogic>().ShieldTrigger(damage, wasAttack);
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
        audioClip ??= logic.audioManager.attackResolution;
        logic.audioManager.NewAudioPrefab(audioClip);
        gm.ClearAttackTargetImages();

        if (damage != 0)
        {
            if (targetState == TargetState.Spot)
                if(logic.type == Type.Fighter)
                    damage *= 2;
            currentHp -= damage;
            logic.audioManager.SelectCharacterDamageSFX(logic.id);
            gm.StateChange(GameState.Damaged);
                logic.StatAdjustment(damage, Status.Damage);
            yield return new WaitForSeconds(1f);
        }
        StatChangeHistoryEntry statChangeLog = new(Status.Damage, damage)
        {
            logIndex = gm.gameLogHistoryEntries.Count,
            loggedCard = gm.currentFocusCardLogic,
            loggedLocation = logic.currentLocation
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
        logic.cardController.AIManager.isPerformingAction = false;
        logic.cardController.enemy.AIManager.isPerformingAction = false;
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
                throw new MissingReferenceException($"Error at stat type {status} for {logic.cardName}");
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
            loggedLocation = logic.currentLocation
        };

        gm.gameLogHistoryEntries.Add(statChangeLog);
        logic.StatAdjustment(healAmount, Status.Heal);
        if (GetComponent<MonsterLogic>() != null)
            GetComponent<MonsterLogic>().OnFieldHpRefresh();
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
                    logics.Add(d.applierLogic.GetComponent<CombatantLogic>());
            return logics;
        }

        //else check for taunters and stealthed units then return valids
        bool tauntEnemy = false;
        int stealthEnemyCount = 0;
        CombatantLogic hero = logic.cardController.enemy.heroCardLogic.GetComponent<CombatantLogic>();
        if (hero.targetState == TargetState.Taunt)
        {
            tauntEnemy = true;
            logics.Add(hero);
        }
        if (hero.targetState == TargetState.Stealth)
            stealthEnemyCount++;

        //first check if there is a taunter
        foreach (CardLogic card in logic.cardController.enemy.fieldLogicList)
        {
            CombatantLogic combatantLogic = card.GetComponent<CombatantLogic>();
            if (combatantLogic.targetState == TargetState.Taunt)
            {
                tauntEnemy = true;
                break;
            }
        }

        //then you can add based on taunt and stealth
        foreach (CardLogic cardLogic in logic.cardController.enemy.fieldLogicList)
        {
            CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
            if (tauntEnemy)
                if (combatantLogic.targetState != TargetState.Taunt)
                    continue;
            if (combatantLogic.targetState==TargetState.Stealth)
            {
                stealthEnemyCount++;
                continue;
            }
            //make sure it's not already there
            if (!logics.Contains(combatantLogic))
                logics.Add(combatantLogic);
        }

        if (tauntEnemy == false)
            logics.Add(hero);

        //if all ally fghters are stealthed, then they are basically all free targets
        if (stealthEnemyCount > 0 && logics.Count == 0)
            foreach (CardLogic cardLogic in logic.cardController.enemy.fieldLogicList)
                logics.Add(cardLogic.GetComponent<CombatantLogic>());

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
            if (targetStatus.fieldIconHolder != null)
                targetStatus.fieldIconHolder.DestroyThis();
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
        logic.cardController.SetStatusIcon(logic.locationOrderNumber, status);
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
        logic.cardController.SetStatusIcon(logic.locationOrderNumber, buff);
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
        logic.cardController.SetStatusIcon(logic.locationOrderNumber, debuff);
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

    public bool ImmobilityCheck()
    {
        if (cardStatuses.Count < 1)
            return false;
        if (DebuffCheck(Debuffs.Stunned)!=null)
            return true;
       if(DebuffCheck(Debuffs.Sleeping) != null)
            return true;
        return false;
    }

    public bool ValidAttackerCheck()
    {
        if (ImmobilityCheck())
            return false;
        if (attacksLeft <= 0)
            return false;
        if (currentAtk <= 0)
            return false;
        return true;
    }

    public void DeclareAttack()
    {
        Debug.Log(logic.cardName);
        attacksLeft -= 1;
        hasAttacked = true;
        hasAttackedThisTurn = true;
        gm.StateChange(GameState.AttackDeclaration);
        validTargets = new(GetValidAttackTargets());
        foreach(CombatantLogic combatantLogic in validTargets)
        {
            Debug.Log(combatantLogic.logic.cardName);
            if (combatantLogic.logic.type == Type.Fighter)
                combatantLogic.logic.cardController.attackTargets[combatantLogic.logic.locationOrderNumber].SetActive(true);
            if (combatantLogic.logic.type == Type.God)
                combatantLogic.logic.cardController.heroAttackTarget.SetActive(true);
        }
        logic.SetFocusCardLogic();
        //handle attacks randomly for AI, needs work
        if(logic.cardController.isAI)
        {
            int ranNum = Random.Range(0, validTargets.Count);
            validTargets[ranNum].AttackTargetAcquisition();
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