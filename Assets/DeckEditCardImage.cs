using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckEditCardImage : MonoBehaviour, IPointerClickHandler
{
    [Header("General Card Info")]
    public DeckEditManager deckEditManager;
    public CardLogic cardLogic;
    public Image cardImage, cardBorderImage;
    public TMP_Text atkText, hpText, costText;
   
    [Header("Deck Edit Info")]
    public TMP_Text cardCountText;
    public Button addCardButton, removeCardButton;
    public int amountOwned, cardCount;
    public bool shouldShowAddButton, shouldShowRemoveButton, isDeckCard;

    public void SetLogic(CardLogic logic, DeckEditManager deckEditManager, bool greyscale)
    {
        cardLogic = logic;
        this.deckEditManager = deckEditManager;
        cardImage.sprite = cardLogic.visualsLogic.image;
        cardImage.color = greyscale ? Color.grey : Color.white;
        cardBorderImage.sprite = cardLogic.visualsLogic.cardImageBorder.GetComponent<SpriteRenderer>().sprite;
        atkText.gameObject.SetActive(cardLogic.dataLogic.type == Type.Fighter);
        hpText.gameObject.SetActive(cardLogic.dataLogic.type == Type.Fighter);
        if (cardLogic.dataLogic.type == Type.Fighter)
        {
            CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
            atkText.text = combatantLogic.atk.ToString();
            hpText.text = combatantLogic.hp.ToString();
        }
        PlayableLogic playableLogic = cardLogic.GetComponent<PlayableLogic>();
        costText.text = playableLogic.cost.ToString();
        if (addCardButton == null)
            return;
        ToggleAddRemoveButtons(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (deckEditManager.currentFocusDeckEditCardImage != null)
            deckEditManager.currentFocusDeckEditCardImage.ToggleAddRemoveButtons(false);
        ToggleAddRemoveButtons(true);
        deckEditManager.currentFocusDeckEditCardImage = this;
    }

    public void ToggleAddRemoveButtons(bool isShow)
    {
        addCardButton.gameObject.SetActive(shouldShowAddButton && isShow);
        removeCardButton.gameObject.SetActive(shouldShowRemoveButton && isShow);
    }

    public void AddCardToDeckScroll()
    {
        deckEditManager.AddCardToDeckScroll(cardLogic);
        ToggleAddRemoveButtons(true);
    }
    public void RemoveCardFromDeckScroll()
    {
        deckEditManager.RemoveCardFromDeckScroll(cardLogic);
        ToggleAddRemoveButtons(true);
    }
}
