using System;
using static CardLogic;
using static Effect;

public class DebuffHistoryEntry
{
	public CardLogic debuffLogic;
	public Effect debuffEffect;
	public int debuffAmount;
	public Location debuffLocation;

	public DebuffHistoryEntry(CardLogic cardLogic, Effect value, int amount, Location location)
	{
		debuffLogic = cardLogic;
		debuffEffect = value;
		debuffAmount = amount;
		debuffLocation = location;
	}
}