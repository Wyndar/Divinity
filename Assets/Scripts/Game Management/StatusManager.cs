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
        if (!cardStatus.shouldCountdown)
        {
            cardStatus.hasDoneCountDownThisTurn = true;
            combatantLogic.TurnTimer();
            return;
        }
        cardStatus.Timer--;
        cardStatus.TimerActions(gm);
        cardStatus.hasDoneCountDownThisTurn = true;
        if (cardStatus.fieldIconHolder != null)
            cardStatus.fieldIconHolder.durationText.text = cardStatus.Timer.ToString();
        if (cardStatus.Timer > 0)
            return;
        cardStatus.DetonateActions(gm);
        combatantLogic.cardStatuses.Remove(cardStatus);
        if (cardStatus.fieldIconHolder == null)
        {
            combatantLogic.TurnTimer();
            return;
        }
        cardStatus.fieldIconHolder.transform.SetParent(null);
        Destroy(cardStatus.fieldIconHolder.gameObject);
        ui.StatIconUpdate(cardLogic);
        combatantLogic.TurnTimer();
        return;
    }
}

