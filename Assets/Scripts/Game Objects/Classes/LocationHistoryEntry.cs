[System.Serializable]
public class LocationHistoryEntry: GameLogHistoryEntry
{
	public Location CurrentLocation;
	public Location PreviousLocation;

	public LocationHistoryEntry(Location currentLocation, Location previousLocation)
	{
		CurrentLocation = currentLocation;
		PreviousLocation = previousLocation;
	}
}