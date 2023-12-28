[System.Serializable]
public class StatChangeHistoryEntry: GameLogHistoryEntry
{
	public Status StatusChange;
	public int ChangeAmount;
	public Location ChangeLocation;

	public StatChangeHistoryEntry(Status status, int amount, Location location)
	{
		StatusChange = status;
		ChangeAmount = amount;
		ChangeLocation = location;
	}
}