[System.Serializable]
public class Debuff
{
    public Debuffs debuff;
    public CardLogic debuffer;
    public CardLogic debuffed;
    public int debuffTimer;
    public bool countdown;
    public bool hasCountdown;

    public Debuff(Debuffs debuf, CardLogic debufferLogic, CardLogic debuffedLogic, int timer, bool timed)
    {
        debuff = debuf;
        debuffer = debufferLogic;
        debuffed = debuffedLogic;
        debuffTimer = timer;
        countdown = timed;
    }

    public void CountdownReset() => hasCountdown = false;

    //to avoid changed list errors
    public void Countdown()
    {
        CombatantLogic combatantLogic = debuffed.GetComponent<CombatantLogic>();
        if (!countdown)
        {
            combatantLogic.TurnTimer();
            return;
        }
        debuffTimer--;
        hasCountdown = true;
        if (debuffTimer > 0)
            return;
        switch (debuff)
        {
            case Debuffs.Bombed:
                combatantLogic.TakeDamage(3, false);
                if (debuffed.currentLocation == Location.Grave)
                    break;
                Debuff stun = new(Debuffs.Stunned, debuffer, debuffed, 2, true);
                combatantLogic.debuffs.Add(stun);
                break;
        }

        combatantLogic.debuffs.Remove(this);
        combatantLogic.TurnTimer();
        return;
    }
}