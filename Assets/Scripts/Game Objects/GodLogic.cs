public class GodLogic : CardLogic
{
    public Game_Manager gm;
    public CombatantLogic combatantLogic;

    public bool hasUsedShield;
    public bool hasUsedShieldThisTurn;

    public int maxShieldUsesPerTurn;
    public int shieldUsesLeft;

    private int incomingDamage;

    public bool ShieldTrigger(int damage)
    {
        if (cardOwner.shieldCount == 0)
            return false;
        if (shieldUsesLeft == 0)
            return false;
        incomingDamage = damage;
        if (cardOwner.isAI)
            HandleAIShield();
        else
            gm.ShowShieldPrompt(cardOwner);
        return true;
    }

    private void HandleAIShield()
    {
        if (cardOwner.AIManager.UseShield(incomingDamage))
            ActivateShield();
        else
            ShieldPass();
    }

    public void ActivateShield()
    {
        cardOwner.shieldCount -= 1;
        gm.StateChange(Game_Manager.GameState.Shielded);
        gm.ClearAttackTargetImages();
        cardController.AIManager.isPerformingAction = false;
        cardController.enemy.AIManager.isPerformingAction = false;
        hasUsedShield = true;
        hasUsedShieldThisTurn = true;
        shieldUsesLeft -= 1;
        gm.GetShieldCard(1, cardOwner);
    }

    public void ShieldPass() => combatantLogic.DamageResolution(incomingDamage, true);

    public void ShieldRefresh()
    {
        if (maxShieldUsesPerTurn > shieldUsesLeft)
            shieldUsesLeft = maxShieldUsesPerTurn;
    }
}

