using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScrollLogEntry : MonoBehaviour
{

    public ScrollingLogPanelHandler scrollingLogPanelHandler;
    public GameLogHistoryEntry historyEntry;
	public GameBattleManager Game_Manager;

    public Image cardHighlightImage;
    public Image cardImage;
    public Image intermediateIconImage;
    public Image targetImage;
    public Image orginalLocationImage;
    public Image targetLocationImage;
    public Image targetHighlightImage;

    public TMP_Text logTypeText;
    public TMP_Text loggedText;
    public TMP_Text targetText;

    private Vector2 cardSize = new(2.2f,3.3f);
    private Vector2 iconSize = new(2f, 2f);

    public Button TargetsButton;
    public List<CardLogic> targets = new();

    public void ButtonTrigger() => scrollingLogPanelHandler.ActivateTargetScroll(this);
    public void SetTargetImage(bool isCard, Sprite intermediateImage, Sprite targetImage, Sprite location)
    {
        Vector2 newSize;
        if(isCard)
        {
            newSize = cardSize;
            targetHighlightImage.sprite = null;
            this.targetImage.gameObject.SetActive(true);
            this.targetImage.sprite = targetImage;
            targetLocationImage.sprite = location;
        }
        else
        {
            newSize = iconSize;
            targetHighlightImage.sprite = targetImage;
            this.targetImage.gameObject.SetActive(false);
        }
        targetText.gameObject.SetActive(false);
        intermediateIconImage.sprite = intermediateImage;
        RectTransform rectTransform = targetHighlightImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = newSize;
    }

    public void RemoveTargetImage()
    {
        targetImage.gameObject.SetActive(false);
        targetText.gameObject.SetActive(false);
        targetHighlightImage.gameObject.SetActive(false);
        intermediateIconImage.gameObject.SetActive(false);
    }
}

