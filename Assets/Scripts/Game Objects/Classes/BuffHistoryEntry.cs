[System.Serializable]
public class BuffHistoryEntry: GameLogHistoryEntry
{
	public int buffAmount;
	public Location buffLocation;

	public BuffHistoryEntry(int amount, Location buffedLocation)
	{ 
		buffAmount = amount;
		buffLocation = buffedLocation;
	}
}

