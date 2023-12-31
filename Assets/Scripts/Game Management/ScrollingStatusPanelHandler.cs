using System.Collections.Generic;
using UnityEngine;

public class ScrollingStatusPanelHandler : MonoBehaviour
{
    [SerializeField]
    private Game_Manager Game_Manager;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private ToolTipManager toolTipManager;

    [SerializeField]
    private List<GameObject> statusImages = new();

    [SerializeField]
    private GameObject spriteHolder;

    public void RemoveStatusImages()
    {
        foreach (GameObject image in statusImages)
            Destroy(image);
        statusImages.Clear();
    }

    public void AddStatusImage(CardStatus status)
    {
        GameObject statusImageObject = Instantiate(spriteHolder, content);
        statusImages.Add(statusImageObject);
        ScrollStatusImage scrollStatusImage = statusImageObject.GetComponent<ScrollStatusImage>();
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
