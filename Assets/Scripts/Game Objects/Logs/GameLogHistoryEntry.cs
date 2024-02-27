using System;
[Serializable]
public class GameLogHistoryEntry
{
    public int logIndex;
    public CardLogic loggedCard;
    public Location loggedLocation;
    public GameLogHistoryEntry() { }

    public GameLogHistoryEntry(int logIndex, CardLogic loggedCard, Location loggedLocation)
    {
        this.logIndex = logIndex;
        this.loggedCard = loggedCard != null ? loggedCard : throw new ArgumentNullException(nameof(loggedCard));
        this.loggedLocation = loggedLocation;
    }
}