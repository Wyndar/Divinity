using System;
public class BuffHistoryEntry
{
	CardLogic buffLogic;
	string buffEffect;
	int buffAmount;
	CardLogic.Location buffLocation;

	public BuffHistoryEntry(CardLogic cardLogic, string value, int amount, CardLogic.Location location)
	{
		buffLogic = cardLogic;
		buffEffect = value;
		buffAmount = amount;
		buffLocation = location;
	}
}

