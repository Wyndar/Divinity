using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingDeckPanelHandler : MonoBehaviour
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private List<GameObject> cardImages = new();

    [SerializeField]
    private GameObject spriteHolder;

    [SerializeField]
    private List<CardLogic> scrollCards = new();

    public Button TargetButton;

    public void AddCardToScrollCards(CardLogic cardLogic) => scrollCards.Add(cardLogic);

    public void AddCardListToScrollCards(List<CardLogic> cardLogics) => scrollCards.AddRange(cardLogics);

    public void ClearScrollCardsList() => scrollCards.Clear();

    public void RemoveContentCards()
    {
        foreach (GameObject image in cardImages)
            Destroy(image);
        cardImages.Clear();
    }

    public void AddContentCards()
    {
        foreach (CardLogic cardLogic in scrollCards)
        {
            GameObject cardImage = Instantiate(spriteHolder, content);
            cardImages.Add(cardImage);
            ScrollCardImage scrollCardImage = cardImage.GetComponent<ScrollCardImage>();
            scrollCardImage.cardLogic = cardLogic;
            scrollCardImage.image.sprite = cardLogic.image;
            scrollCardImage.highlightImage.color = Color.clear;
        }
    }
}
