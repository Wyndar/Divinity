[System.Serializable]
public class GameLogHistoryEntry
{
	public BuffHistoryEntry buffHistoryEntry;
	public DebuffHistoryEntry debuffHistoryEntry;
	public LocationHistoryEntry locationHistoryEntry;
	public StatChangeHistoryEntry statChangeHistoryEntry;
	public CardLogic loggedCard;

	public GameLogHistoryEntry(BuffHistoryEntry bhe, DebuffHistoryEntry dhe,LocationHistoryEntry lhe,StatChangeHistoryEntry sce, CardLogic cardLogic)
	{
		buffHistoryEntry = bhe;
		debuffHistoryEntry = dhe;
		locationHistoryEntry = lhe;
		statChangeHistoryEntry = sce;
		loggedCard = cardLogic;
	}
}

