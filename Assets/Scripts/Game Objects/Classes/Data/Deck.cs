using System.Collections.Generic;

[System.Serializable]
public class Deck
{
    public string DeckName { get; private set; }
    public string GodID { get; private set; }
    public string DisplayCardID { get; private set; }
    public bool Locked { get; private set; }
    public List<string> DeckList { get; private set; }

    public void ToggleLock() => Locked = !Locked;
}