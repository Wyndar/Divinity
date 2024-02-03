using UnityEngine;
[System.Serializable]
public class Taunt : Buff
{
    public Taunt(CardLogic bufferLogic, CardLogic buffedLogic, bool timed, int duration)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<UIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Taunt");
        buff = Buffs.Taunt;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        shouldCountdown = timed;
        Timer = duration;
        sprite = uIManager.tauntSprite;
    }
}