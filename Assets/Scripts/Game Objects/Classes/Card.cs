using System.Collections.Generic;

[System.Serializable]
public class Card
{
    public string Id { get; set; }
    public string CardName { get; set; }
    public string CardType { get; set; }

    public string CardText { get; set; }
    public string CardFlavorText { get; set; }

    public List<string> Traits { get; set; }

    //if !god
    public int Cost { get; set; }

    //if !spell
    public int Atk { get; set; }
    public int Hp { get; set; }
    public int MaxAttacks { get; set; }

    public List<Effect> Effects { get; set; }
}