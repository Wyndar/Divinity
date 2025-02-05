using UnityEngine;
[System.Serializable]
public class Camouflage : Buff
{
    public Camouflage(CardLogic bufferLogic, CardLogic buffedLogic, int duration)
    {
        toolTipManager = GameObject.FindFirstObjectByType<ToolTipManager>();
        uIManager = GameObject.FindFirstObjectByType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Camouflage");
        buff = Buffs.Camouflage;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        shouldCountdown = duration != 0;
        Timer = duration;
        sprite = uIManager.camouflageSprite;
    }
}