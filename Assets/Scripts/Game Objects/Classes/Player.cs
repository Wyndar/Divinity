[System.Serializable]
public class Player
{
    public string PlayerName { get; private set; }
    public string PlayerID { get; private set; }
    public string DeckPath { get; private set; }
    public string HeroDeckPath { get; private set; }

    public bool IsAI { get; private set; }
    public bool IsLocal { get; private set; }
}

