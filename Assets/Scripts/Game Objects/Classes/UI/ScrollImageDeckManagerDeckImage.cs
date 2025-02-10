using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollImageDeckManagerDeckImage : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text deckName;
    public Image deckImage, highlightImage, lockImage;
    public Deck deck;
    public TMP_InputField deckNameInput;
    public GameObject deckEditButton, nameEditButton, deleteButton, lockButton;
    public Sprite defaultImage;
    public DeckManager deckManager;
    private const int maxSize = 27, setSize = 24;

    public void SetDeckImage(Deck deck, DeckManager deckManager)
    {
        this.deck = deck;
        deckName.text = deck.DeckName;
        if (deckName.text.Length > maxSize)
            deckName.text = deckName.text.Remove(setSize) + "...";
        Sprite sprite = Resources.Load($"Sprites And Visuals/Card Images/{deck.DisplayCardID}", typeof(Sprite)) as Sprite;
        deckImage.sprite = sprite != null ? sprite : defaultImage;
        this.deckManager = deckManager;
        lockImage.sprite = deck.Locked ? deckManager.lockSprite : deckManager.unlockSprite;
        deckImage.color = deck.Locked ? Color.grey : Color.white;
        lockButton.SetActive(deck.Locked);
        HighlightDeck(false);
    }

    public void HighlightDeck(bool isHighlighted) => highlightImage.color = isHighlighted ? Color.yellow : Color.clear;

    public void OnPointerClick(PointerEventData eventData)
    {
        HighlightDeck(highlightImage.color == Color.yellow);
        if (deckNameInput.gameObject.activeSelf)
            return;
        if (deck.Locked)
        {
            deckManager.MessagePanel("Deck is locked!\n Unlock to edit.");
            return;
        }
        deckEditButton.SetActive(!deckEditButton.activeSelf);
        nameEditButton.SetActive(!nameEditButton.activeSelf);
        deleteButton.SetActive(!deleteButton.activeSelf);
        lockButton.SetActive(!lockButton.activeSelf);
    }
    public void EditDeck() => deckManager.OpenDeckEdit(deck);
    public void ToggleDeckLock()
    {
        deck.ToggleLock();
        deckManager.UpdateDeck();
        deckManager.MessagePanel(deck.Locked ? "Deck is locked!" : "Deck is unlocked!");
        lockImage.sprite = deck.Locked ? deckManager.lockSprite : deckManager.unlockSprite;
        deckImage.color = deck.Locked ? Color.grey : Color.white;
        DisableButtons();
    }
    public void DeckNameUpdate()
    { 
        deckName.text = deckNameInput.text;
        deck.SetDeckName(deckNameInput.text);
        deckNameInput.gameObject.SetActive(false);
        deckManager.UpdateDeck();
        deckManager.MessagePanel("Deck name updated!");
    }
    public void ActivateEdit() 
    {
        deckNameInput.gameObject.SetActive(true); 
        DisableButtons();
    }

    private void DisableButtons()
    {
        deleteButton.SetActive(false);
        lockButton.SetActive(deck.Locked);
        deckEditButton.SetActive(false);
        nameEditButton.SetActive(false);
    }
    public void DeleteDeck() => deckManager.DeleteDeck(deck);
}
