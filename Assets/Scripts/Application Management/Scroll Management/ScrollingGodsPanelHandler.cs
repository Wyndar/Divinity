using UnityEngine;

public class ScrollingGodsPanelHandler : GenericScrollingPanelHandler<CardLogic>
{
    [SerializeField] private DeckManager deckManager;

    protected override void SetupImage(GameObject imageObject, CardLogic cardLogic)
    {
        ScrollImageDeckManagerGodImage scrollGodImage = imageObject.GetComponent<ScrollImageDeckManagerGodImage>();
        scrollGodImage.SetGodImage(cardLogic, deckManager, true);
    }
}
