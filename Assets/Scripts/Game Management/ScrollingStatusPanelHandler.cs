//fundamentally the same thing as the other loghandler, wet only for readability and ease of access
//do not optimize to dry

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

    [SerializeField]
    private List<string> statAdjusterName = new();

    public void AddCardNameToStatAdjusterNames(CardLogic cardLogic) => statAdjusterName.Add(cardLogic.cardName);

    public void AddCardNamesToStatAdjusterNames(List<CardLogic> cardLogics)
    {
        foreach (CardLogic cardLogic in cardLogics)
            statAdjusterName.Add(cardLogic.cardName);
    }
    public void ClearStatAdjusterCardsList() => statAdjusterName.Clear();

    public void RemoveStatusImages()
    {
        foreach (GameObject image in statusImages)
            Destroy(image);
        statusImages.Clear();
    }

    public void AddStatusImages(Sprite statusImage, CardLogic statAdjuster)
    {
        GameObject statusImageObject = Instantiate(spriteHolder, content);
        statusImages.Add(statusImageObject);
        ScrollStatusImage scrollStatusImage = statusImageObject.GetComponent<ScrollStatusImage>();
        scrollStatusImage.cardLogic = statAdjuster;
        scrollStatusImage.Game_Manager = Game_Manager;
        scrollStatusImage.image.sprite = statusImage;
        scrollStatusImage.highlightImage.color = Color.clear;
    }
}
