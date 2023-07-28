﻿using UnityEngine;
using System.Collections.Generic;
using static Game_Manager;
using static Buff;
using static StatChangeHistoryEntry;
using static Card;
using UnityEditor.Experimental.GraphView;
using static UnityEngine.GraphicsBuffer;

public class CombatantLogic : MonoBehaviour
{
	public Game_Manager gm;
    public CardLogic logic;

    public List<CombatantLogic> validTargets = new();

    public Buff buffs = new();
    public Debuff debuffs = new();

    public int atk, hp, maxHp, currentAtk, currentHp, armor, maxAttacks, attacksLeft;

    public bool hasAttacked, hasAttackedThisTurn;

    public void TakeDamage(int damage, bool wasAttack)
    {
        bool blockDamage = false;
        if (logic.cardType == "god")
            blockDamage = GetComponent<GodLogic>().ShieldTrigger(damage, wasAttack);
        if (!blockDamage)
            DamageResolution(damage, true);
    }

    public void DamageResolution(int damage, bool wasAttack)
    {
        damage -= armor;
        if (damage != 0)
        {
            currentHp -= damage;
            gm.StateChange(GameState.Damaged);
                logic.StatAdjustment(damage, Status.Damage);
        }
logic.statChangeHistoryEnteries.Add(new(gm.currentFocusCardLogic, gm.currentFocusCardLogic.focusEffect, Status.Damage, damage));
        if (!wasAttack)
            return;

        logic.cardController.AIManager.isPerformingAction = false;
        logic.cardController.enemy.AIManager.isPerformingAction = false;
        gm.ClearAttackTargetImages();
        gm.ChainResolution();
        return;
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
                Debug.Log("Error at stat type" + logic.cardName);
                break;
        }
        if (currentAtk < 0)
            currentAtk = 0;
        logic.StatAdjustment(value, status);

    }

    public void Heal(int healAmount)
    {
        currentHp += healAmount;
        OverhealCheck();
        int prevHp = currentHp - healAmount;
        logic.statChangeHistoryEnteries.Add(new(gm.currentFocusCardLogic, gm.currentFocusCardLogic.focusEffect, Status.Heal, healAmount));
        logic.StatAdjustment(healAmount, Status.Heal);
            GetComponent<MonsterLogic>().OnFieldHpRefresh();
    }

    public void OverhealCheck()
    {
        if (currentHp > maxHp)
            currentHp = maxHp;
    }

    public void AttackResolution()
    {
        CombatantLogic attacker = gm.currentFocusCardLogic.gameObject.GetComponent<CombatantLogic>();
        TakeDamage(attacker.currentAtk, true);
    }

    public List<CombatantLogic> GetValidAttackTargets()
    {
        List<CombatantLogic> logics = new();
        bool tauntEnemy = false;
        int stealthEnemyCount = 0;
        foreach (CardLogic cardLogic in logic.cardController.enemy.fieldLogicList)
        {
            CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
            if (combatantLogic.buffs.targetState==TargetState.Taunt)
            {
                tauntEnemy = true;
                logics.Add(combatantLogic);
            }
            if (tauntEnemy)
                continue;
            if (combatantLogic.buffs.targetState==TargetState.Stealth)
            {
                stealthEnemyCount++;
                continue;
            }
         logics.Add(combatantLogic);
        }
        //if all ally fghters are stealthed, then they are basically all free targets
        if (stealthEnemyCount > 0 && logics.Count == 0)
        {
            foreach (CardLogic cardLogic in logic.cardController.enemy.fieldLogicList)
            {
                CombatantLogic combatant = cardLogic.GetComponent<CombatantLogic>();
                    logics.Add(combatant);
            }
        }
        if (tauntEnemy)
            return logics;
        logics.Add(logic.cardController.enemy.heroCardLogic.GetComponent<CombatantLogic>());
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
            if (combatantLogic.logic.cardType == "monster")
                combatantLogic.logic.cardController.attackTargets[combatantLogic.logic.locationOrderNumber].SetActive(true);
            if (combatantLogic.logic.cardType == "god")
                combatantLogic.logic.cardController.heroAttackTarget.SetActive(true);
        }
        gm.currentFocusCardLogic = logic;
        //handle attacks randomly for AI, needs work
        if(logic.cardController.isAI)
        {
            int ranNum = Random.Range(0, validTargets.Count);
            validTargets[ranNum].AttackTargetAcquisition();
        }
    }

    public void AttackRefresh() => attacksLeft = maxAttacks;
}