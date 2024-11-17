using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UnderworldManager : MonoBehaviour, IPointerClickHandler
{
    public Game_Manager manager;
    
    public PlayerManager player;

    public GameObject image, front, back, outline, border, canvas;

    public CardLogic topCard;

    public TMP_Text nameText, effectText, costText, ATKText, HPText;

    public void ResetTopCard()
    {
        if (player.graveLogicList.Count == 0)
        {
            image.SetActive(false);
            front.SetActive(false);
            back.SetActive(false);
            outline.SetActive(false);
            border.SetActive(false);
            canvas.SetActive(false);
            return;
        }

        topCard = player.graveLogicList[^1];
        image.SetActive(true);
        front.SetActive(true);
        back.SetActive(true);
        border.SetActive(true);
        canvas.SetActive(true);

        image.GetComponent<SpriteRenderer>().sprite = topCard.image;
        front.GetComponent<SpriteRenderer>().sprite = topCard.cardFace.GetComponent<SpriteRenderer>().sprite;
        back.GetComponent<SpriteRenderer>().sprite = topCard.cardBack.GetComponent<SpriteRenderer>().sprite;
        outline.GetComponent<SpriteRenderer>().sprite = topCard.cardOutline.GetComponent<SpriteRenderer>().sprite;
        border.GetComponent<SpriteRenderer>().sprite = topCard.cardImageBorder.GetComponent<SpriteRenderer>().sprite;
        nameText.text = topCard.cardName;
        effectText.text = topCard.effectText.text;
        costText.text = topCard.costText.text;
        ATKText.text = topCard.type == Type.Fighter ? topCard.GetComponent<CombatantLogic>().atk.ToString() : "";
        HPText.text = topCard.type == Type.Fighter ? topCard.GetComponent<CombatantLogic>().hp.ToString() : "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (player.graveLogicList.Count == 0)
            return;
        if (manager.gameState != GameState.Open)
            return;
        if (manager.isActivatingEffect)
            return;
        if (manager.isPlayingCard)
            return;
        topCard.SetFocusCardLogic();
        
    }
}
