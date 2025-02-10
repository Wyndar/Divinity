using UnityEngine;

public class ScrollingDecksPanelHandler : GenericScrollingPanelHandler<Deck>
{
    [SerializeField] private Transform addButton;
    [SerializeField] private DeckManager deckManager;

    protected override void SetupImage(GameObject imageObject, Deck deck)
    {
        ScrollImageDeckManagerDeckImage scrollDeckImage = imageObject.GetComponent<ScrollImageDeckManagerDeckImage>();
        scrollDeckImage.SetDeckImage(deck, deckManager);
    }

    public void AddDecks()
    {
        AddContent();
        addButton.SetAsLastSibling();
    }
}
