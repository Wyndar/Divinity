using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingCardPaneHandler : MonoBehaviour
{
    [SerializeField]
    private Game_Manager Game_Manager;

    [SerializeField]
    private UXManager UXManager;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private GameObject spriteHolder;

    [SerializeField]
    private List<CardLogic> scrollCards = new();

    public void AddCardToScrollCards(CardLogic cardLogic) => scrollCards.Add(cardLogic);

    public void AddCardListToScrollCards(List<CardLogic> cardLogics) => scrollCards.AddRange(cardLogics);

    public void ClearScrollCardsList() => scrollCards.Clear();

    public void RemoveContentCards()
    {
        foreach (Transform transform in content)
        {
            transform.SetParent(null);
            Destroy(transform.gameObject);
        }
    }

    public void AddContentCards()
    {
        foreach (CardLogic cardLogic in scrollCards)
        {
            GameObject cardImage = Instantiate(spriteHolder, content);
            cardImage.GetComponent<Image>().sprite = cardLogic.image;
            ScrollCardImage scrollCardImage = cardImage.AddComponent<ScrollCardImage>();
            scrollCardImage.cardLogic = cardLogic;
            scrollCardImage.Game_Manager = Game_Manager;
            scrollCardImage.UXManager = UXManager;
        }
    }
}
