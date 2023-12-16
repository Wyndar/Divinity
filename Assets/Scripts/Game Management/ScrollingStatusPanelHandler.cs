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
        scrollStatusImage.durationText.text = status.Timer > 0 ? status.Timer.ToString() : " ";
        scrollStatusImage.image.color = status is Buff ? Color.green : Color.red;
        scrollStatusImage.highlightImage.color = Color.clear;
    }
}
