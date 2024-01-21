using UnityEngine;
[System.Serializable]
public class Burn : Debuff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Debuffs debuffApplied = Debuffs.Burned;
    public ToolTipInfo toolTip; 

    public Burn(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Burn");
        debuff = debuffApplied;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer > 0 ? timer : 2;
        shouldCountdown = true;
        canDetonate = true;
        toolTipInfo = toolTip;
        sprite = UIManager.burnSprite;
    }
    public override void TimerActions(Game_Manager gm)
    {
        base.TimerActions(gm);
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.TakeDamage(2, false);
    }

    public override void DetonateActions(Game_Manager gm)
    {
        if (Timer <= 0)
            return;
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.TakeDamage(Timer * 2, false);
        gm.StateChange(GameState.Detonate);
    }
}
