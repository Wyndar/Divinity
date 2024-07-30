using UnityEngine;
[System.Serializable]
public class Stealth : Buff
{
    public Stealth(CardLogic bufferLogic, CardLogic buffedLogic, int duration)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Stealth");
        buff = Buffs.Stealth;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        shouldCountdown = duration != 0;
        Timer = duration;
        sprite = uIManager.stealthSprite;
    }
}