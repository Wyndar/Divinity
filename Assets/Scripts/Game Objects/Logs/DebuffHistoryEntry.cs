[System.Serializable]
public class DebuffHistoryEntry: GameLogHistoryEntry
{
	public int debuffAmount;
	public Location debuffLocation;

	public DebuffHistoryEntry(int amount, Location location)
	{
		debuffAmount = amount;
		debuffLocation = location;
	}
}