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
        gm.ShowShieldPrompt(cardOwner);
        return true;
    }

    public void ActivateShield()
    {
        cardOwner.shieldCount -= 1;
        gm.StateChange(Game_Manager.GameState.Shielded);
        gm.GetShieldCard(1, cardOwner);
        hasUsedShield = true;
        hasUsedShieldThisTurn = true;
        shieldUsesLeft -= 1;
    }

    public void ShieldPass() => combatantLogic.DamageResolution(incomingDamage);

    public void ShieldRefresh()
    {
        if (maxShieldUsesPerTurn > shieldUsesLeft)
            shieldUsesLeft = maxShieldUsesPerTurn;
    }
}

