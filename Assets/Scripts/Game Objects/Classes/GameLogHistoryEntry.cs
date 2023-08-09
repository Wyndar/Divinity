[System.Serializable]
public class GameLogHistoryEntry
{
    public LogType log;
    public int logIndex;
    public CardLogic loggedCard;
    public Effect loggedEffect;
    public EffectsUsed loggedEffectUsed;
}