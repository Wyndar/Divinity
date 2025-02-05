using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckEditCardImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public CardLogic cardLogic;
    public Image cardImage;
    public Image cardBorderImage;
    public TMP_Text atkText;
    public TMP_Text hpText;
    public TMP_Text costText;
    public DeckEditManager deckEditManager;
    public Button addCardButton;
    public Button removeCardButton;
    public int amountOwned;
    public int cardCount;
    public TMP_Text cardCountText;

    public void SetLogic(CardLogic logic, DeckEditManager deckEditManager)
    {
        cardLogic = logic;
        this.deckEditManager = deckEditManager;
        cardImage.sprite = cardLogic.visualsLogic.image;
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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
