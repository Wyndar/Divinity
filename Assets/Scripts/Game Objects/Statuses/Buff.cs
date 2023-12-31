using UnityEngine;
[System.Serializable]
public class Buff:CardStatus
{
    public Buffs buff;

    public Buff()
    {

    }
    public Buff(Buffs buf, CardLogic bufferLogic, CardLogic buffedLogic, int timer, bool timed, ToolTipInfo toolTip, Sprite bufSprite)
    {
        buff = buf;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        toolTipInfo = toolTip;
        sprite = bufSprite;
    }
}