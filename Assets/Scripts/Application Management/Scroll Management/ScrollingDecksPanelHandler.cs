using System.Collections.Generic;
using UnityEngine;

public class ScrollingDecksPanelHandler : MonoBehaviour
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private List<GameObject> deckImages = new();

    [SerializeField]
    private GameObject spriteHolder;

    [SerializeField]
    private List<Deck> scrollDecks = new();

    public void AddDeckToScrollDecks(Deck deck) => scrollDecks.Add(deck);

    public void AddDeckListToScrollDecks(List<Deck> decks) => scrollDecks.AddRange(decks);

    public void ClearScrollDecksList() => scrollDecks.Clear();

    public void RemoveContentDecks()
    {
        foreach (GameObject image in deckImages)
            Destroy(image);
        deckImages.Clear();
    }

    public void AddDecks(DeckManager deckManager)
    {
        foreach (Deck deck in scrollDecks)
        {
            GameObject deckImage = Instantiate(spriteHolder, content);
            deckImages.Add(deckImage);
            ScrollImageDeckManagerDeckImage scrollCardImage = deckImage.GetComponent<ScrollImageDeckManagerDeckImage>();
            scrollCardImage.SetDeckImage(deck, deckManager);
        }
    }
}
