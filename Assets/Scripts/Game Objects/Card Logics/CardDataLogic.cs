using System.Collections.Generic;
using UnityEngine;

public class CardDataLogic : MonoBehaviour
{
    public Type type;
    public Rarity rarity;
    public List<PlayType> playTypes = new();
    public List<Trait> traits = new();
    public List<Attunement> attunements = new();

    public string id, cardName, cardText, flavorText;
    public PlayerManager cardOwner, cardController;

    public Location currentLocation;
    public int locationOrderNumber;
    private CardLogic cardLogic;
    public void Initialize() => cardLogic = GetComponent<CardLogic>();
    public void ControllerSwap(PlayerManager player)
    {
        cardController = player;
        gameObject.transform.rotation = player.deck.transform.rotation;
    }

    public void LocationChange(Location location, int num)
    {
        
        LocationHistoryEntry locationLog = new(location)
        {
            logIndex = cardLogic.gameManager.gameLogHistoryEntries.Count,
            loggedCard = cardLogic,
            loggedLocation = currentLocation
        };
        cardLogic.gameManager.gameLogHistoryEntries.Add(locationLog);
        currentLocation = location;
        locationOrderNumber = num;
    }
}