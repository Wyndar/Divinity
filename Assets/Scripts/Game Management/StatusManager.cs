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
        cardStatus.hasDoneCountDownThisTurn = true;
        if (cardStatus.Timer > 0)
            return;
        if (cardStatus is Debuff d)
        {
            switch (d.debuff)
            {
                case Debuffs.Bombed:
                    combatantLogic.TakeDamage(3, false);
                    gm.StateChange(GameState.Detonate);
                    if (cardLogic.currentLocation == Location.Grave)
                        break;
                    Stun stun = new(d.applierLogic, d.affectedLogic, 3, true);
                    combatantLogic.cardStatuses.Add(stun);
                    cardLogic.EffectLogger(EffectsUsed.Stun, 1, LogType.Debuff, null);
                    cardLogic.cardController.SetStatusIcon(cardLogic.locationOrderNumber, stun);
                    gm.StateChange(GameState.Stun);
                    break;
            }
        }
        combatantLogic.cardStatuses.Remove(cardStatus);
        if (cardStatus.fieldIconHolder == null)
        {
            combatantLogic.TurnTimer();
            return;
        }
        cardStatus.fieldIconHolder.durationText.text = cardStatus.shouldCountdown ? cardStatus.Timer.ToString() : " ";
        cardStatus.fieldIconHolder.transform.SetParent(null);
        Destroy(cardStatus.fieldIconHolder.gameObject);
        ui.StatIconUpdate(cardLogic);
        combatantLogic.TurnTimer();
        return;
    }
}

