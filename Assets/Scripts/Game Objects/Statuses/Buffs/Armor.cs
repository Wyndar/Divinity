using UnityEngine;
[System.Serializable]
public class Armor : Buff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Buffs buffApplied = Buffs.Armor;
    public ToolTipInfo toolTip;
    public Sprite debufSprite;
    public Armor(CardLogic bufferLogic, CardLogic buffedLogic, int amount)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Armor");
        buff = buffApplied;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        Amount = amount;
        toolTipInfo = toolTip;
        sprite = UIManager.armorSprite;
       
    }
}