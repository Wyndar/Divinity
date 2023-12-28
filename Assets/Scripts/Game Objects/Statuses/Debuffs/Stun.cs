using UnityEngine;
[System.Serializable]
public class Stun : Debuff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Debuffs debuffApplied = Debuffs.Stunned;
    public ToolTipInfo toolTip; 
    public Sprite debufSprite;
    public Stun(CardLogic debufferLogic, CardLogic debuffedLogic, int timer, bool timed)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Stun");
        debuff = debuffApplied;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        toolTipInfo = toolTip;
        sprite = UIManager.bombSprite;
    }
}
