using static Card;
using static PlayerManager;

public class GodLogic : CardLogic
{
    public Game_Manager gm;
    public CombatantLogic combatantLogic;

    public bool hasUsedShield;
    public bool hasUsedShieldThisTurn;

    public int maxShieldUsesPerTurn;
    public int shieldUsesLeft;

    private int incomingDamage;
    private bool wasAttack;

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
        gm.StateChange(Game_Manager.GameState.Shielded);
        gm.ClearAttackTargetImages();
        cardController.AIManager.isPerformingAction = false;
        cardController.enemy.AIManager.isPerformingAction = false;
        hasUsedShield = true;
        hasUsedShieldThisTurn = true;
        shieldUsesLeft -= 1;
        gm.GetShieldCard(1, cardOwner);
        if (!wasAttack)
        {
            gm.isWaitingForResponse = false; gm.currentFocusCardLogic.FinishResolution(gm.currentFocusCardLogic.effectCountNumber, gm.currentFocusCardLogic.subCountNumber);
        }
    }

    public void ShieldPass()
    {
        combatantLogic.DamageResolution(incomingDamage, true);
        if (wasAttack)
            return;
        gm.isWaitingForResponse = false;
        gm.currentFocusCardLogic.FinishResolution(gm.currentFocusCardLogic.effectCountNumber, gm.currentFocusCardLogic.subCountNumber);
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
            GameManager.GameOver(cardController.enemy);
        }
    }

    override public void StatAdjustment(int value, Status status)
    {
        cardController.SetHeroStatus(locationOrderNumber, status, value);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
    }


    public void OnFieldAtkRefresh() => cardController.heroAtkText.text = combatantLogic.currentAtk.ToString();
    public void OnFieldHpRefresh() => cardController.heroHpText.text = combatantLogic.currentHp.ToString();
}