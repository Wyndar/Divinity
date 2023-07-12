using System;
using static CardLogic;
public class BuffHistoryEntry
{
	CardLogic buffLogic;
	Effect buffEffect;
	int buffAmount;
	CardLogic.Location buffLocation;

	public BuffHistoryEntry(CardLogic cardLogic, Effect value, int amount, Location location)
	{
		buffLogic = cardLogic;
		buffEffect = value;
		buffAmount = amount;
		buffLocation = location;
	}
}

