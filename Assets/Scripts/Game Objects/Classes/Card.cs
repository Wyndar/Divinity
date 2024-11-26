using System.Collections.Generic;

[System.Serializable]
public class Card
{
    public string Id { get; private set; }
    public string CardName { get; private set; }
    public Type CardType { get; private set; }

    public Rarity Rarity { get; private set; }

    public string CardText { get; private set; }
    public string CardFlavorText { get; private set; }

    public List<Trait> Traits { get; private set; }
    public List<Attunement> Attunements { get; private set; }

    public List<int>AttunementRates { get; private set; }

    //if !god
    public int Cost { get; private set; }

    //if !spell
    public int Atk { get; private set; }
    public int Hp { get; private set; }
    public int MaxAttacks { get; private set; }

    public List<Effect> Effects { get; private set; }
}