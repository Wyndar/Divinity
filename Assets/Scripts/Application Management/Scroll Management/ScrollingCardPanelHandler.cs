using UnityEngine;

public class ScrollingCardPanelHandler : GenericScrollingPanelHandler<CardLogic>
{
    [SerializeField] private GameBattleManager Game_Manager;

    protected override void SetupImage(GameObject imageObject, CardLogic cardLogic)
    {
        ScrollCardImage scrollCardImage = imageObject.GetComponent<ScrollCardImage>();
        scrollCardImage.cardLogic = cardLogic;
        scrollCardImage.Game_Manager = Game_Manager;
        scrollCardImage.image.sprite = cardLogic.visualsLogic.image;
        scrollCardImage.highlightImage.color = Color.clear;
    }
}
