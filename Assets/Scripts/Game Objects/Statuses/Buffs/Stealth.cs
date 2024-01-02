using UnityEngine;
[System.Serializable]
public class Stealth : Buff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Buffs buffApplied = Buffs.Stealth;
    public ToolTipInfo toolTip;
    public Stealth(CardLogic bufferLogic, CardLogic buffedLogic, bool timed, int duration)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Stealth");
        buff = buffApplied;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        shouldCountdown = timed;
        Timer = duration;
        toolTipInfo = toolTip;
        sprite = UIManager.stealthSprite;
    }
}