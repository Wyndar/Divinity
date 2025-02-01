[System.Serializable]
public class StatChangeHistoryEntry: GameLogHistoryEntry
{
	public Status StatusChange;
	public int ChangeAmount;

	public StatChangeHistoryEntry(Status status, int amount)
	{
		StatusChange = status;
		ChangeAmount = amount;
	}
}