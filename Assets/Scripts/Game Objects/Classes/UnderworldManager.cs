using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnderworldManager : MonoBehaviour, IPointerClickHandler
{
    public Game_Manager manager;
    
    public PlayerManager player;

    public GameObject image;
    public GameObject front;
    public GameObject back;
    public GameObject outline;

    public CardLogic topCard;

    public void ResetTopCard()
    {
        if (player.graveLogicList.Count == 0)
        {
            image.SetActive(false);
            front.SetActive(false);
            back.SetActive(false);
            outline.SetActive(false);
            return;
        }
            
        topCard = player.graveLogicList[^1];
        image.SetActive(true);
        front.SetActive(true);
        back.SetActive(true);

        image.GetComponent<SpriteRenderer>().sprite = topCard.image;
        front.GetComponent<SpriteRenderer>().sprite = topCard.cardFace.GetComponent<SpriteRenderer>().sprite;
        back.GetComponent<SpriteRenderer>().sprite = topCard.cardBack.GetComponent<SpriteRenderer>().sprite;
        outline.GetComponent<SpriteRenderer>().sprite = topCard.cardOutline.GetComponent<SpriteRenderer>().sprite;
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
