using UnityEngine;

public class ScrollingStatusPanelHandler : GenericScrollingPanelHandler<CardStatus>
{
    [SerializeField] private GameBattleManager Game_Manager;
    [SerializeField] private ToolTipManager toolTipManager;

    protected override void SetupImage(GameObject imageObject, CardStatus status)
    {
        ScrollStatusImage scrollStatusImage = imageObject.GetComponent<ScrollStatusImage>();
        scrollStatusImage.Game_Manager = Game_Manager;
        scrollStatusImage.toolTipManager = toolTipManager;
        scrollStatusImage.currentStatus = status;
        scrollStatusImage.image.sprite = status.sprite;
        scrollStatusImage.durationText.text = status.shouldCountdown ? status.Timer.ToString() : " ";
        scrollStatusImage.amountText.text = status.Amount > 0 ? status.Amount.ToString() : " ";
        scrollStatusImage.image.color = status is Buff ? Color.blue : Color.red;
        scrollStatusImage.highlightImage.color = Color.clear;
    }
}
