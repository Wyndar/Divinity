using System;
using static CardLogic;
using static Effect;
public class LocationHistoryEntry
{
	CardLogic CardLogic;
	EffectsUsed Effect;
	Location CurrentLocation;
	Location PreviousLocation;

	public LocationHistoryEntry(CardLogic cardLogic, EffectsUsed value, Location currentLocation, Location previousLocation)
	{
		CardLogic = cardLogic;
		Effect = value;
		CurrentLocation = currentLocation;
		PreviousLocation = previousLocation;
	}
}