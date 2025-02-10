using UnityEngine;
using UnityEngine.UI;

public class ScrollingTargetsPanelHandler : GenericScrollingPanelHandler<CardLogic>
{
    [SerializeField] private GameBattleManager Game_Manager;
    public Button TargetButton;

    protected override void SetupImage(GameObject imageObject, CardLogic cardLogic)
    {
        ScrollCardImage scrollCardImage = imageObject.GetComponent<ScrollCardImage>();
        scrollCardImage.cardLogic = cardLogic;
        scrollCardImage.Game_Manager = Game_Manager;
        scrollCardImage.image.sprite = cardLogic.visualsLogic.isFaceDown
            ? cardLogic.visualsLogic.cardBack.GetComponent<SpriteRenderer>().sprite
            : cardLogic.visualsLogic.image;
        scrollCardImage.highlightImage.color = Color.clear;
    }
}
