using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GodLogic : CardLogic
{
    public GameBattleManager gm;
    public CombatantLogic combatantLogic;

    public bool hasUsedShield;
    public bool hasUsedShieldThisTurn;

    public int maxShieldUsesPerTurn;
    public int shieldUsesLeft;

    private int incomingDamage;
    private bool wasAttack;

    public List<int> attunementRates = new();
    public bool ShieldTrigger(int damage, bool wasAttack)
    {
        if (dataLogic.cardOwner.shieldCount == 0)
            return false;
        if (shieldUsesLeft == 0)
            return false;
        incomingDamage = damage;
        this.wasAttack = wasAttack;
        if (!wasAttack)
            gm.isWaitingForResponse = true;
        if (dataLogic.cardOwner.isAI)
            HandleAIShield();
        else
            gm.ShowShieldPrompt(dataLogic.cardOwner);
        return true;
    }

    private void HandleAIShield()
    {
        if (dataLogic.cardOwner.AIManager.UseShield(incomingDamage, wasAttack))
            ActivateShield();
        else
            ShieldPass();
    }

    public void ActivateShield()
    {
        dataLogic.cardOwner.shieldCount -= 1;
        dataLogic.cardOwner.SetShield(1, 1);
        gm.StateChange(GameState.Shielded);
        gm.ClearAttackTargetImages();
        dataLogic.cardController.AIManager.isPerformingAction = false;
        dataLogic.cardController.enemy.AIManager.isPerformingAction = false;
        hasUsedShield = true;
        hasUsedShieldThisTurn = true;
        shieldUsesLeft -= 1;
        StartCoroutine(gm.GetShieldCard(1, dataLogic.cardOwner));
        if (!wasAttack)
            gm.isWaitingForResponse = false;
    }

    public void ShieldPass()
    {
        StartCoroutine(combatantLogic.DamageResolution(incomingDamage, true));
        if (wasAttack)
            return;
        gm.isWaitingForResponse = false;
    }

    public void ShieldRefresh()
    {
        if (maxShieldUsesPerTurn > shieldUsesLeft)
            shieldUsesLeft = maxShieldUsesPerTurn;
    }

    public void LoseCheck()
    {
        if (combatantLogic.currentHp <= 0)
        {
            combatantLogic.currentHp = 0;
            gm.GameOver(dataLogic.cardController.enemy);
        }
    }

    override public void StatAdjustment(int value, Status status)
    {
        dataLogic.cardController.SetHeroStatus(status, value);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
        LoseCheck();
    }


    public void OnFieldAtkRefresh()
    {
        dataLogic.cardController.heroAtkText.color = combatantLogic.atk != combatantLogic.currentAtk ? combatantLogic.currentAtk 
            < combatantLogic.atk ? Color.red : Color.blue : Color.black;
        dataLogic.cardController.heroAtkText.text = combatantLogic.currentAtk.ToString();
    }
    public void OnFieldHpRefresh()
    {
        dataLogic.cardController.heroHpText.color = combatantLogic.currentHp != combatantLogic.maxHp ? combatantLogic.currentHp
            < combatantLogic.maxHp ? Color.red : Color.blue : Color.black;
        dataLogic.cardController.heroHpText.text = combatantLogic.currentHp.ToString();
    }
}