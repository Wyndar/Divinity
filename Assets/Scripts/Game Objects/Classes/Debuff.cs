using UnityEngine;
[System.Serializable]
public class Debuff:CardStatus
{
    public Debuffs debuff;

    public Debuff(Debuffs debuf, CardLogic debufferLogic, CardLogic debuffedLogic, int timer, bool timed, ToolTipInfo toolTip, Sprite debufSprite)
    {
        debuff = debuf;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        toolTipInfo = toolTip;
        sprite = debufSprite;
    }
}