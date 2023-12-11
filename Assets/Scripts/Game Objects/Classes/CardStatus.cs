using UnityEngine;

[System.Serializable]
public abstract class CardStatus
{
    public CardLogic affectedLogic;
    public CardLogic applierLogic;
    public int Timer;
    public Sprite sprite;
    public bool shouldCountdown;
    public bool hasDoneCountDownThisTurn;

    public CardStatus()
    {

    }

    public CardStatus(CardLogic bufferLogic, CardLogic buffedLogic, int timer, bool timed, Sprite bufSprite)
    {
        affectedLogic = bufferLogic;
        applierLogic = buffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        sprite = bufSprite;
    }
}
