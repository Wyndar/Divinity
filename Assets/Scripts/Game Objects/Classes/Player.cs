[System.Serializable]
public class Player
{
    public string PlayerName { get; set; }
    public string PlayerID { get; set; }
    public string DeckPath { get; set; }
    public string HeroDeckPath { get; set; }

    public bool IsAI { get; set; }
    public bool IsLocal { get; set; }
}

