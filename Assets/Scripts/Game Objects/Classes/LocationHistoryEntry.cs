using static CardLogic;
using static Effect;

[System.Serializable]
public class LocationHistoryEntry
{
	public CardLogic CardLogic;
	public Effect Effect;
	public EffectsUsed changeType;
	public Location CurrentLocation;
	public Location PreviousLocation;

	public LocationHistoryEntry(CardLogic cardLogic, Effect value, EffectsUsed changeEffect, Location currentLocation, Location previousLocation)
	{
		CardLogic = cardLogic;
		Effect = value;
		changeType = changeEffect;
		CurrentLocation = currentLocation;
		PreviousLocation = previousLocation;
	}
}