using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollImageDeckManagerGodImage : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text godName;
    public Image godImage;
    public CardLogic godLogic;
    public Image highlightImage;
    public GameObject lockImage;
    public DeckManager deckManager;

    public void SetGodImage(CardLogic godLogic, DeckManager deckManager, bool isUnlocked = false)
    {
        this.godLogic = godLogic;
        godName.text = godLogic.dataLogic.cardName;
        godImage.sprite = godLogic.visualsLogic.image;
        this.deckManager = deckManager;
        lockImage.SetActive(!isUnlocked);
        godImage.color = !isUnlocked ? Color.grey : Color.white;
        HighlightCard(false);
    }

    public void HighlightCard(bool isHighlighted) => highlightImage.color = isHighlighted ? Color.yellow : Color.clear;

    public void OnPointerClick(PointerEventData eventData) => deckManager.DisplayDeckScroll(godLogic);

    public void OnPointerEnter(PointerEventData eventData) => HighlightCard(true);

    public void OnPointerExit(PointerEventData eventData) => HighlightCard(false);
}
