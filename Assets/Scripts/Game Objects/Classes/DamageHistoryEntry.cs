using System;
using static CardLogic;
public class DamageHistoryEntry
{
	CardLogic CardLogic;
	Effect Effect;
	int PreviousHP;
	int CurrentHP;

	public DamageHistoryEntry(CardLogic cardLogic, Effect value, int previousHP, int currentHP)
	{
		CardLogic = cardLogic;
		Effect = value;
		PreviousHP = previousHP;
		CurrentHP = currentHP;
	}
}

