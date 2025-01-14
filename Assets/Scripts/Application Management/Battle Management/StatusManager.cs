using TMPro;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public GameBattleManager gm;
    public SecondaryUIManager ui;
    public GameObject animatedTimerPrefab;

    public void CountdownReset(CardStatus cardStatus) => cardStatus.hasDoneCountDownThisTurn = false;

    //to avoid changed list errors
    public void Countdown(CardStatus cardStatus)
    {
        CardLogic cardLogic = cardStatus.affectedLogic;
        CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
        cardStatus.TimerActions(gm, animatedTimerPrefab);
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

