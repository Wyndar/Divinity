using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public Game_Manager gm;
    public UIManager ui;

    public void CountdownReset(CardStatus cardStatus) => cardStatus.hasDoneCountDownThisTurn = false;

    //to avoid changed list errors
    public void Countdown(CardStatus cardStatus)
    {
        CombatantLogic combatantLogic = cardStatus.applierLogic.GetComponent<CombatantLogic>();
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
        if (cardStatus is Debuff)
        {
            Debuff d = (Debuff)cardStatus;
            switch (d.debuff)
            {
                case Debuffs.Bombed:
                    combatantLogic.TakeDamage(3, false);
                    if (d.affectedLogic.currentLocation == Location.Grave)
                        break;
                    Debuff stun = new(Debuffs.Stunned, d.applierLogic, d.affectedLogic, 2, true, gm.ToolTipManager.tooltipInfos.Find(a => a.key == "Bomb"), ui.stunSprite);
                    combatantLogic.cardStatuses.Add(stun);
                    break;
            }
        }
        combatantLogic.cardStatuses.Remove(cardStatus);
        combatantLogic.TurnTimer();
        return;
    }
}

