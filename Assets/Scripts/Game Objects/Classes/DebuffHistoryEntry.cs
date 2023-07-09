using System;
using static CardLogic;

public class DebuffHistoryEntry
{
	CardLogic debuffLogic;
	string debuffEffect;
	int debuffAmount;
	Location debuffLocation;

	public DebuffHistoryEntry(CardLogic cardLogic, string value, int amount, Location location)
	{
		debuffLogic = cardLogic;
		debuffEffect = value;
		debuffAmount = amount;
		debuffLocation = location;
	}
}

