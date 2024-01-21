using TMPro;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public Game_Manager gm;
    public UIManager ui;

    public void CountdownReset(CardStatus cardStatus) => cardStatus.hasDoneCountDownThisTurn = false;

    //to avoid changed list errors
    public void Countdown(CardStatus cardStatus)
    {
        CardLogic cardLogic = cardStatus.affectedLogic;
        CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
        cardStatus.TimerActions(gm);
        if (cardStatus.Timer <= 0 && cardStatus.shouldCountdown)
        {
            cardStatus.DetonateActions(gm);
            combatantLogic.cardStatuses.Remove(cardStatus);
            ui.StatIconUpdate(cardLogic);
            return;
        }
        combatantLogic.TurnTimer();
        return;
    }
}

