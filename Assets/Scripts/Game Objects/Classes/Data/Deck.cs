using System.Collections.Generic;

[System.Serializable]
public class Deck
{
    public string DeckName { get; private set; }
    public string GodID { get; private set; }
    public string DisplayCardID { get; private set; }
    public bool Locked { get; private set; }
    public List<CardOwnedID> DeckList { get; private set; }

    public void ToggleLock() => Locked = !Locked;
    public void SetDeckName(string name) => DeckName = name;
    public void SetGodID(string godID) => GodID = godID;
    public void SetDisplayCardID(string displayCardID) => DisplayCardID = displayCardID;
    public void SetDeckList(List<CardOwnedID> deckList) => DeckList = deckList;
}