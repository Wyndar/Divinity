using System.Linq;
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
                    Stun stun = new(d.applierLogic, d.affectedLogic, 2, true);
                    gm.StateChange(GameState.Stun);
                    combatantLogic.cardStatuses.Add(stun);
                    cardLogic.EffectLogger(EffectsUsed.Stun, 1, LogType.Debuff, null);
                    break;
            }
        }
        combatantLogic.cardStatuses.Remove(cardStatus);
        if(!combatantLogic.DebuffCheck(Debuffs.Bombed))
            cardLogic.cardController.bombIcons[cardLogic.locationOrderNumber].SetActive(false);
        combatantLogic.TurnTimer();
        return;
    }
}

