using UnityEngine;
using System.Collections.Generic;

public class CombatantLogic : MonoBehaviour
{
	public Game_Manager gm;
    public CardLogic logic;

    public int atk, hp, maxHp, currentAtk, currentHp, maxAttacks, attacksLeft;

    public bool hasAttacked, hasAttackedThisTurn;

    public void TakeDamage(int damage)
    {
        bool blockDamage = false;
        if (logic.cardType == "god")
            blockDamage = GetComponent<GodLogic>().ShieldTrigger(damage);
        if (!blockDamage)
            DamageResolution(damage);    
    }

    public void DamageResolution(int damage)
    {
        currentHp -= damage;
        gm.StateChange(Game_Manager.GameState.Damaged);
        if (logic.cardType == "monster")
        {
            GetComponent<MonsterLogic>().OnFieldHpRefresh();
            GetComponent<MonsterLogic>().DeathCheck();
        }
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
            GetComponent<MonsterLogic>().OnFieldAtkRefresh();
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
                GetComponent<MonsterLogic>().DeathCheck();
        }
        if (logic.cardType == "monster")
            GetComponent<MonsterLogic>().OnFieldHpRefresh();
    }

    public void Heal(int healAmount)
    {
        currentHp += healAmount;
        OverhealCheck();
        if (logic.cardType == "monster")
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
        attacker.attacksLeft -= 1;
        attacker.hasAttacked = true;
        attacker.hasAttackedThisTurn = true;
        TakeDamage(attacker.currentAtk);
        gm.ChainResolution();
    }

    public List<CombatantLogic> GetValidAttackTargets()
    {
        List<CombatantLogic> logics = new(FindObjectsOfType<CombatantLogic>());
        List<CombatantLogic> returnList = new();
        foreach(CombatantLogic logic in logics)
        {
            CardLogic target = logic.GetComponent<CardLogic>();
            //remove ally cards
            if (target.cardController == GetComponent<CardLogic>().cardController)
                continue;
            //remove cards not on field
            if (target.currentLocation != CardLogic.Location.Field)
                continue;
            //implement taunt type effects here

            //if it passes all tests, add it to the list
            returnList.Add(logic);
        }
        return returnList;
    }

    public void AttackTargetAcquisition()
    {
        if (gm.gameState == Game_Manager.GameState.AttackDeclaration)
        {
            List<CombatantLogic> allTargetsList = gm.currentFocusCardLogic.GetComponent<CombatantLogic>().GetValidAttackTargets();
           if(allTargetsList.Contains(this))
                    AttackResolution();
            if (gm.currentFocusCardLogic.gameObject.GetComponent<CombatantLogic>().attacksLeft == 0)
            {
                gm.StateChange(Game_Manager.GameState.Open);
                return;
            }
        }
    }

    public void DeclareAttack() => gm.gameState = Game_Manager.GameState.AttackDeclaration;

    public void AttackRefresh()
    {
        if(attacksLeft < maxAttacks)
            attacksLeft = maxAttacks;
    }
}