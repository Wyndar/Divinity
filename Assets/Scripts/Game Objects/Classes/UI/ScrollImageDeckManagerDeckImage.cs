using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollImageDeckManagerDeckImage : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text deckName;
    public Image deckImage;
    public Deck deck;
    public Image highlightImage;
    public Image lockImage;
    public GameObject deckEditButton;
    public GameObject nameEditButton;

    public DeckManager deckManager;
    private const int maxSize = 27;
    private const int setSize = 24;

    public void SetDeckImage(Deck deck, DeckManager deckManager)
    {
        this.deck = deck;
        deckName.text = deck.DeckName;
        if (deckName.text.Length > maxSize)
            deckName.text = deckName.text.Remove(setSize) + "...";
        deckImage.sprite = Resources.Load($"Sprites And Visuals/Card Images/{deck.DisplayCardID}", typeof(Sprite)) as Sprite; ;
        this.deckManager = deckManager;
        lockImage.sprite = deck.Locked ? deckManager.lockSprite : deckManager.unlockSprite;
        HighlightDeck(false);
    }

    public void HighlightDeck(bool isHighlighted) => highlightImage.color = isHighlighted ? Color.yellow : Color.clear;

    public void OnPointerClick(PointerEventData eventData)
    {
        HighlightDeck(highlightImage.color == Color.yellow);
        deckEditButton.SetActive(!deckEditButton.activeSelf);
        nameEditButton.SetActive(!nameEditButton.activeSelf);
    }
    public void EditDeck() => deckManager.OpenDeckEdit(deck);
    public void ToggleDeckLock()
    {
        deck.ToggleLock();
        deckManager.UpdateDeck();
        lockImage.sprite = !deck.Locked ? deckManager.unlockSprite : deckManager.lockSprite;
    }
}
