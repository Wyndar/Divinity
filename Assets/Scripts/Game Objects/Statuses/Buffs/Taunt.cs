using UnityEngine;
[System.Serializable]
public class Taunt : Buff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Buffs buffApplied = Buffs.Taunt;
    public ToolTipInfo toolTip;
    public Sprite debufSprite;
    public Taunt(CardLogic bufferLogic, CardLogic buffedLogic, int amount)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Taunt");
        buff = buffApplied;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        Amount = amount;
        toolTipInfo = toolTip;
        sprite = UIManager.tauntSprite;
    }
}