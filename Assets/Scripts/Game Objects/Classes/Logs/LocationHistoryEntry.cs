[System.Serializable]
public class LocationHistoryEntry: GameLogHistoryEntry
{
	public Location CurrentLocation;

	public LocationHistoryEntry(Location currentLocation) => CurrentLocation = currentLocation;
}