[System.Serializable]
public class Buff
{
    public Buffs buff;
    public CardLogic buffer;
    public CardLogic buffed;
    public int buffTimer;
    public bool countdown;
    public bool hasCountDown;

    public Buff(Buffs buf, CardLogic bufferLogic, CardLogic buffedLogic, int timer, bool timed)
    {
        buff = buf;
        buffer = bufferLogic;
        buffed = buffedLogic;
        buffTimer = timer;
        countdown = timed;
    }

    public void CountdownReset() => hasCountDown = false;

    public void Countdown()
    {
        if (!countdown)
            return;

        buffTimer--;
    }
}