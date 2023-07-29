using static CardLogic;
using static Effect;

[System.Serializable]
public class BuffHistoryEntry
{
	public CardLogic buffLogic;
	public Effect buffEffect;
	public EffectsUsed buff;
	public int buffAmount;
	public Location buffLocation;

	public BuffHistoryEntry(CardLogic buffingCard, Effect buffingEffect, EffectsUsed buf, int amount, Location buffedLocation)
	{
		buffLogic = buffingCard;
		buffEffect = buffingEffect;
		buff = buf;
		buffAmount = amount;
		buffLocation = buffedLocation;
	}
}

