﻿using NUnit.Framework;
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
        if (cardOwner.shieldCount == 0)
            return false;
        if (shieldUsesLeft == 0)
            return false;
        incomingDamage = damage;
        this.wasAttack = wasAttack;
        if (!wasAttack)
            gm.isWaitingForResponse = true;
        if (cardOwner.isAI)
            HandleAIShield();
        else
            gm.ShowShieldPrompt(cardOwner);
        return true;
    }

    private void HandleAIShield()
    {
        if (cardOwner.AIManager.UseShield(incomingDamage, wasAttack))
            ActivateShield();
        else
            ShieldPass();
    }

    public void ActivateShield()
    {
        cardOwner.shieldCount -= 1;
        cardOwner.SetShield(1, 1);
        gm.StateChange(GameState.Shielded);
        gm.ClearAttackTargetImages();
        cardController.AIManager.isPerformingAction = false;
        cardController.enemy.AIManager.isPerformingAction = false;
        hasUsedShield = true;
        hasUsedShieldThisTurn = true;
        shieldUsesLeft -= 1;
        StartCoroutine(gm.GetShieldCard(1, cardOwner));
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
            gm.GameOver(cardController.enemy);
        }
    }

    override public void StatAdjustment(int value, Status status)
    {
        cardController.SetHeroStatus(status, value);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
        LoseCheck();
    }


    public void OnFieldAtkRefresh()
    {
        cardController.heroAtkText.color = combatantLogic.atk != combatantLogic.currentAtk ? combatantLogic.currentAtk 
            < combatantLogic.atk ? Color.red : Color.blue : Color.black;
        cardController.heroAtkText.text = combatantLogic.currentAtk.ToString();
    }
    public void OnFieldHpRefresh()
    {
        cardController.heroHpText.color = combatantLogic.currentHp != combatantLogic.maxHp ? combatantLogic.currentHp
            < combatantLogic.maxHp ? Color.red : Color.blue : Color.black;
        cardController.heroHpText.text = combatantLogic.currentHp.ToString();
    }
}