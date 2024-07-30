using UnityEngine;
[System.Serializable]
public class Taunt : Buff
{
    public Taunt(CardLogic bufferLogic, CardLogic buffedLogic, int duration)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Taunt");
        buff = Buffs.Taunt;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        shouldCountdown = duration != 0;
        Timer = duration;
        sprite = uIManager.tauntSprite;
    }
}