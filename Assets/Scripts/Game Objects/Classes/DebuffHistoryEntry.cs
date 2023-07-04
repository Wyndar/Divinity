using System;
public class DebuffHistoryEntry
{
	CardLogic debuffLogic;
	string debuffEffect;
	int debuffAmount;
	CardLogic.Location debuffLocation;

	public DebuffHistoryEntry(CardLogic cardLogic, string value, int amount, CardLogic.Location location)
	{
		debuffLogic = cardLogic;
		debuffEffect = value;
		debuffAmount = amount;
		debuffLocation = location;
	}
}

