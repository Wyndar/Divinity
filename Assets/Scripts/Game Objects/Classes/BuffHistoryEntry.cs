using System;
using static CardLogic;
public class BuffHistoryEntry
{
	public CardLogic buffLogic;
	public Effect buffEffect;
	public int buffAmount;
	public Location buffLocation;

	public BuffHistoryEntry(CardLogic cardLogic, Effect value, int amount, Location location)
	{
		buffLogic = cardLogic;
		buffEffect = value;
		buffAmount = amount;
		buffLocation = location;
	}
}

