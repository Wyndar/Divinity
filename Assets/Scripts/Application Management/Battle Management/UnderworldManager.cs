using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UnderworldManager : MonoBehaviour, IPointerClickHandler
{
    public GameBattleManager manager;
    
    public PlayerManager player;

    public GameObject image, back, outline, border, canvas;

    public CardLogic topCard;

    public TMP_Text costText, ATKText, HPText;

    public void ResetTopCard()
    {
        if (player.graveLogicList.Count == 0)
        {
            image.SetActive(false);
            back.SetActive(false);
            outline.SetActive(false);
            border.SetActive(false);
            canvas.SetActive(false);
            return;
        }

        topCard = player.graveLogicList[^1];
        image.SetActive(true);
        back.SetActive(true);
        border.SetActive(true);
        canvas.SetActive(true);

        image.GetComponent<SpriteRenderer>().sprite = topCard.image;
        back.GetComponent<SpriteRenderer>().sprite = topCard.cardBack.GetComponent<SpriteRenderer>().sprite;
        outline.GetComponent<SpriteRenderer>().sprite = topCard.cardOutline.GetComponent<SpriteRenderer>().sprite;
        border.GetComponent<SpriteRenderer>().sprite = topCard.cardImageBorder.GetComponent<SpriteRenderer>().sprite;
        costText.text = topCard.costText.text;
        ATKText.text = topCard.type == Type.Fighter ? topCard.GetComponent<CombatantLogic>().atk.ToString() : "";
        HPText.text = topCard.type == Type.Fighter ? topCard.GetComponent<CombatantLogic>().hp.ToString() : "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (player.graveLogicList.Count == 0 || manager.gameState != GameState.Open || manager.isActivatingEffect)
            return;
        if (manager.isPlayingCard)
            return;
        topCard.SetFocusCardLogic();
        
    }
}
