using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TextScrollerHandler : MonoBehaviour
{
    public TextMeshProUGUI scrollingTMP;
    public TextMeshProUGUI scrollingTMPClone;
    private const float scrollSpeed = 50f;
    private float width;
    private float halfWidth;
    private RectTransform textRT;
    private RectTransform cloneRT;
    private Vector3 startPos;

    private void Awake()
    {
        textRT = scrollingTMP.GetComponent<RectTransform>();
        startPos = textRT.anchoredPosition;
        SpawnNewText(); 
    }

    private void Update()
    {
        if(scrollingTMP.havePropertiesChanged)
        {
            width = scrollingTMP.preferredWidth;
            scrollingTMPClone.text = scrollingTMP.text;
            halfWidth = GetComponent<RectTransform>().sizeDelta.x / 2;
        }
        if (scrollingTMP.text.Length < 11)
        {
            scrollingTMPClone.gameObject.SetActive(false);
            textRT.anchoredPosition = startPos;
            return;
        }
        scrollingTMPClone.gameObject.SetActive(true);
        if (textRT != null)
            textRT.anchoredPosition = Vector2.MoveTowards(textRT.anchoredPosition, new Vector2(
                textRT.anchoredPosition.x - (width + halfWidth), textRT.anchoredPosition.y), scrollSpeed * Time.deltaTime);
        if (cloneRT != null)
            cloneRT.anchoredPosition = new Vector2(textRT.anchoredPosition.x + width + halfWidth, textRT.anchoredPosition.y);

        if (textRT.anchoredPosition.x <= width * -1 + startPos.x)
        {
            Destroy(scrollingTMP.gameObject);
            scrollingTMP = scrollingTMPClone;
            textRT = scrollingTMP.GetComponent<RectTransform>();
            scrollingTMPClone = null;
            cloneRT = null;
            SpawnNewText();
        }
    }
    public void SpawnNewText()
    {
        if (scrollingTMPClone != null)
            return;
        scrollingTMP.gameObject.name = "Text";
        halfWidth = GetComponent<RectTransform>().sizeDelta.x/2;
        width = scrollingTMP.preferredWidth;
        scrollingTMPClone = Instantiate(scrollingTMP, transform);
        scrollingTMPClone.gameObject.name="Text Clone";
        cloneRT = scrollingTMPClone.GetComponent<RectTransform>();
        cloneRT.anchoredPosition = new Vector2(textRT.anchoredPosition.x + width + halfWidth, textRT.anchoredPosition.y);
    }
}
