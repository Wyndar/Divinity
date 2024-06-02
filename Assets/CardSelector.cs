using System;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    [SerializeField]
    private CardMaker CardMaker;

    [SerializeField]
    private GameObject exitButton, cardPrefab;

    private GameObject card;
    private void OnEnable() => exitButton.SetActive(card);
    public void NewCard(string typeString)
    {
        Type type = Enum.Parse<Type>(typeString, true);
        if (card != null)
            Destroy(card);
        card = Instantiate(cardPrefab);
        card.SetActive(false);
        
        CardMaker.gameObject.SetActive(true);
        CardMaker.NewCard(type, card);
        gameObject.SetActive(false);
    }

    public void DisableScreen()
    {
        CardMaker.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
