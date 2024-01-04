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

    public int atk, hp, maxHp, currentAtk, currentHp, armor, maxAttacks, attacksLeft;

    public bool hasAttacked, hasAttackedThisTurn, hasDoneCountdown;

    private Coroutine currentCoroutine;

    public void TakeDamage(int damage, bool wasAttack)
    {
        bool blockDamage = false;
        if (logic.type == Type.God)
            blockDamage = GetComponent<GodLogic>().ShieldTrigger(damage, wasAttack);
        if (!blockDamage)
            currentCoroutine = StartCoroutine(DamageResolution(damage, wasAttack));
    }

    public IEnumerator DamageResolution(int damage, bool wasAttack)
    {
        damage -= armor;
        if (armor > 0)
            logic.audioManager.NewAudioPrefab(logic.audioManager.attackResolutionArmored);
        else
            logic.audioManager.NewAudioPrefab(logic.audioManager.attackResolution);
        gm.ClearAttackTargetImages();
        if (damage != 0)
        {
            currentHp -= damage;
            logic.audioManager.SelectCharacterDamageSFX(logic.id);
            gm.StateChange(GameState.Damaged);
                logic.StatAdjustment(damage, Status.Damage);
            yield return new WaitForSeconds(1f);
        }
        Effect damager = null;
        EffectsUsed damagingEffect = EffectsUsed.Undefined;
        if(gm.isActivatingEffect)
        {
            if (gm.currentFocusCardLogic == null)
                Debug.Log("Is this a game mechanic?");
            else
            {
                damager = gm.currentFocusCardLogic.focusEffect;
                damagingEffect = gm.currentFocusCardLogic.focusEffect.effectsUsed[gm.currentFocusCardLogic.subCountNumber];
            }
        }
        StatChangeHistoryEntry statChangeLog = new(Status.Damage, damage, logic.currentLocation)
        {
            log = LogType.StatChange,
            logIndex = logic.statChangeHistoryEnteries.Count,
            loggedCard = gm.currentFocusCardLogic,
            loggedEffect = damager,
            loggedEffectUsed = damagingEffect
        };
        logic.statChangeHistoryEnteries.Add(statChangeLog);
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
                Debug.Log($"Error at stat type {logic.cardName}");
                break;
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
        StatChangeHistoryEntry statChangeLog = new(Status.Damage, healAmount, logic.currentLocation)
        {
            log = LogType.StatChange,
            logIndex = logic.statChangeHistoryEnteries.Count,
            loggedCard = gm.currentFocusCardLogic,
            loggedEffect = gm.currentFocusCardLogic.focusEffect,
            loggedEffectUsed = gm.currentFocusCardLogic.focusEffect.effectsUsed[gm.currentFocusCardLogic.subCountNumber]
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
        TakeDamage(attacker.currentAtk, true);
    }

    public List<CombatantLogic> GetValidAttackTargets()
    {
        List<CombatantLogic> logics = new();

        //if provoked, just return the provoker
        if (DebuffCheck(Debuffs.Provoked))
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

        foreach (CardLogic cardLogic in logic.cardController.enemy.fieldLogicList)
        {
            CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
            if (combatantLogic.targetState==TargetState.Taunt)
            {
                tauntEnemy = true;
                logics.Add(combatantLogic);
            }
            if (tauntEnemy)
                continue;
            if (combatantLogic.targetState==TargetState.Stealth)
            {
                stealthEnemyCount++;
                continue;
            }
         logics.Add(combatantLogic);
        }

        if (tauntEnemy == false)
            logics.Add(hero);
        
        //if all ally fghters are stealthed, then they are basically all free targets
        if (stealthEnemyCount > 0 && logics.Count == 0)
        {
            foreach (CardLogic cardLogic in logic.cardController.enemy.fieldLogicList)
            {
                CombatantLogic combatant = cardLogic.GetComponent<CombatantLogic>();
                    logics.Add(combatant);
            }
        }
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
        if (targetStatus != null)
        {
            if (targetStatus.fieldIconHolder != null)
                Destroy(targetStatus.fieldIconHolder.gameObject);
            cardStatuses.Remove(targetStatus);
        }
    }
    public void SetTargetStatus(CardStatus status, TargetState state)
    {
        RemoveTargetStatus();
        targetStatus = status;
        targetState = state;
        cardStatuses.Add(status);
    }

    public bool DebuffCheck(Debuffs debuff)
    {
        if (cardStatuses.Count < 1)
            return false;

        foreach (CardStatus status in cardStatuses)
            if (status is Debuff d)
                if (d.debuff == debuff)
                    return true;

        return false;
    }

    public bool ImmobilityCheck()
    {
        if (cardStatuses.Count < 1)
            return false;
        if (DebuffCheck(Debuffs.Stunned))
            return true;
       if(DebuffCheck(Debuffs.Sleeping))
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
            if (combatantLogic.logic.cardType == "monster")
                combatantLogic.logic.cardController.attackTargets[combatantLogic.logic.locationOrderNumber].SetActive(true);
            if (combatantLogic.logic.cardType == "god")
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