using UnityEngine;
using System.Collections.Generic;
using static Game_Manager;
using static Buff;
using static PlayerManager;
using UnityEditor.Experimental.GraphView;

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
            if (logic.cardType == "monster")
            {
                logic.cardController.SetStatus(logic.locationOrderNumber, Status.Damage, damage);
GetComponent<MonsterLogic>().OnFieldHpRefresh();
                GetComponent<MonsterLogic>().DeathCheck();
            }
            else
                GetComponent<GodLogic>().LoseCheck();
        }
        if (!wasAttack)
            return;
        //slow down attack stack trace for AI till coroutine for atk animation is done

        logic.cardController.AIManager.isPerformingAction = false;
        logic.cardController.enemy.AIManager.isPerformingAction = false;
        gm.ClearAttackTargetImages();
        gm.ChainResolution();
        return;
    }

    public void ATKAdjustment(int value, bool add)
    {
        if (add)
            currentAtk += value;
        else
            currentAtk -= value;
        if (currentAtk < 0)
            currentAtk = 0;
        if (logic.cardType == "monster")
        {
            if (add)
                logic.cardController.SetStatus(logic.locationOrderNumber, Status.AtkGain, value);
            else
                logic.cardController.SetStatus(logic.locationOrderNumber, Status.AtkLoss, value);
            GetComponent<MonsterLogic>().OnFieldAtkRefresh();
        }
    }

    public void MaxHPAdjustment(int value, bool add)
    {
        if (add)
        {
            maxHp += value;
            currentHp += value;
        }
        else
        {
            maxHp -= value;
            currentHp -= value;
            if (logic.cardType == "monster")
            {
                if (add)
                    logic.cardController.SetStatus(logic.locationOrderNumber, Status.HpGain, value);
                else
                    logic.cardController.SetStatus(logic.locationOrderNumber, Status.HpLoss, value);
                GetComponent<MonsterLogic>().DeathCheck();
            }
        }
        if (logic.cardType == "monster")
            GetComponent<MonsterLogic>().OnFieldHpRefresh();
    }

    public void Heal(int healAmount)
    {
        currentHp += healAmount;
        OverhealCheck();
        if (logic.cardType == "monster")
        {
            logic.cardController.SetStatus(logic.locationOrderNumber, Status.Heal, healAmount);
            GetComponent<MonsterLogic>().OnFieldHpRefresh();
        }
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