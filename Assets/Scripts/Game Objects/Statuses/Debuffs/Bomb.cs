using UnityEngine;
[System.Serializable]
public class Bomb : Debuff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Debuffs debuffApplied = Debuffs.Bombed;
    public ToolTipInfo toolTip; 
    public Bomb(CardLogic debufferLogic, CardLogic debuffedLogic, int timer, bool timed)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Bomb");
        debuff = debuffApplied;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        toolTipInfo = toolTip;
        sprite = UIManager.bombSprite;
    }
}
