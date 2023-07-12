using System;
using static CardLogic;
using static Effect;

public class DebuffHistoryEntry
{
	CardLogic debuffLogic;
	Effect debuffEffect;
	int debuffAmount;
	Location debuffLocation;

	public DebuffHistoryEntry(CardLogic cardLogic, Effect value, int amount, Location location)
	{
		debuffLogic = cardLogic;
		debuffEffect = value;
		debuffAmount = amount;
		debuffLocation = location;
	}
}