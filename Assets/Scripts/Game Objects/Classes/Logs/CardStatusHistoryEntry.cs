[System.Serializable]
public class CardStatusHistoryEntry: GameLogHistoryEntry
{
	public CardStatus cardStatus;
	public CardStatusHistoryEntry(CardStatus status) => cardStatus = status;
}