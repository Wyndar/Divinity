using UnityEngine;
[System.Serializable]
public class Poison : Debuff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Debuffs debuffApplied = Debuffs.Poisoned;
    public ToolTipInfo toolTip; 

    public Poison(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Poison");
        debuff = debuffApplied;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer > 0 ? timer : 4;
        shouldCountdown = true;
        canDetonate = true;
        toolTipInfo = toolTip;
        sprite = UIManager.poisonSprite;
    }
    public override void TimerActions(Game_Manager gm)
    {
        base.TimerActions(gm);
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.StatAdjustment(1, Status.HpLoss);
    }

    public override void DetonateActions(Game_Manager gm)
    {
        if (Timer <= 0)
            return;
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.StatAdjustment(Timer, Status.HpLoss);
        gm.StateChange(GameState.Detonate);
    }
}
