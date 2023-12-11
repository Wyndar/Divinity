using System.Collections.Generic;
using UnityEngine;

public class ScrollingStatusPanelHandler : MonoBehaviour
{
    [SerializeField]
    private Game_Manager Game_Manager;

    [SerializeField]
    private Transform content;

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

    public void AddStatusImage(Sprite statusImage, CardLogic statAdjuster, bool isBuff, int duration)
    {
        GameObject statusImageObject = Instantiate(spriteHolder, content);
        statusImages.Add(statusImageObject);
        ScrollStatusImage scrollStatusImage = statusImageObject.GetComponent<ScrollStatusImage>();
        scrollStatusImage.cardLogic = statAdjuster;
        scrollStatusImage.Game_Manager = Game_Manager;
        scrollStatusImage.image.sprite = statusImage;
        scrollStatusImage.durationText.text = duration > 0 ? duration.ToString() : " ";
        scrollStatusImage.image.color = isBuff ? Color.green : Color.red;
        scrollStatusImage.highlightImage.color = Color.clear;
    }
}
