[System.Serializable]
public class StatChangeHistoryEntry
{
	public CardLogic CardLogic;
	public Effect Effect;
	public Status StatusChange;
	public int ChangeAmount;

	public enum Status
	{
		Heal, Damage, Death, HpLoss, HpGain, AtkLoss, AtkGain, Undefined
	}

	public StatChangeHistoryEntry(CardLogic cardLogic, Effect value, Status status, int amount)
	{
		CardLogic = cardLogic;
		Effect = value;
		StatusChange = status;
		ChangeAmount = amount;
	}
}