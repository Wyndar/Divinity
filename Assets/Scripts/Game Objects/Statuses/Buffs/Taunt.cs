using UnityEngine;
[System.Serializable]
public class Taunt : Buff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Buffs buffApplied = Buffs.Taunt;
    public ToolTipInfo toolTip;
    public Taunt(CardLogic bufferLogic, CardLogic buffedLogic, bool timed, int duration)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Taunt");
        buff = buffApplied;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        shouldCountdown = timed;
        Timer = duration;
        toolTipInfo = toolTip;
        sprite = UIManager.tauntSprite;
    }
}