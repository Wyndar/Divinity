using UnityEngine;
[System.Serializable]
public class Buff:CardStatus
{
    public Buffs buff;

    public Buff(Buffs buf, CardLogic bufferLogic, CardLogic buffedLogic, int timer, bool timed, Sprite bufSprite)
    {
        buff = buf;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        sprite = bufSprite;
    }
}