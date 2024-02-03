using UnityEngine;
[System.Serializable]
public class Armor : Buff
{
    public Armor(CardLogic bufferLogic, CardLogic buffedLogic, int amount)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<UIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Armor");
        buff = Buffs.Armor;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        Amount = amount;
        sprite = uIManager.armorSprite;
       
    }
}