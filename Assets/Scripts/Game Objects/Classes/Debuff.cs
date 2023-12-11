using UnityEngine;
[System.Serializable]
public class Debuff:CardStatus
{
    public Debuffs debuff;

    public Debuff(Debuffs debuf, CardLogic debufferLogic, CardLogic debuffedLogic, int timer, bool timed, Sprite debufSprite)
    {
        debuff = debuf;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        sprite = debufSprite;
    }
}