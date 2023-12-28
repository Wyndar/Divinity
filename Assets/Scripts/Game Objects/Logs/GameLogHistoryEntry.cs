using System;

[Serializable]
public class GameLogHistoryEntry
{
    public LogType log;
    public int logIndex;
    public CardLogic loggedCard;
    public Effect loggedEffect;
    public EffectsUsed loggedEffectUsed;

    public GameLogHistoryEntry() { }

    public GameLogHistoryEntry(LogType log, int logIndex, CardLogic loggedCard, Effect loggedEffect, EffectsUsed loggedEffectUsed)
    {
        this.log = log;
        this.logIndex = logIndex;
        this.loggedCard = loggedCard ?? throw new ArgumentNullException(nameof(loggedCard));
        this.loggedEffect = loggedEffect ?? throw new ArgumentNullException(nameof(loggedEffect));
        this.loggedEffectUsed = loggedEffectUsed;
    }
}