using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ScrollLogEntry : MonoBehaviour
{
    public GameLogHistoryEntry historyEntry;
	public Game_Manager Game_Manager;
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

    //disabled by default, only enabled for multitarget effects
    public TMP_Dropdown targetsDropdown;
    public void EnableDropDown() => targetsDropdown.gameObject.SetActive(true);
    public void DisableDropDown() => targetsDropdown.gameObject.SetActive(false);

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
}

