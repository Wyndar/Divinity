using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TextScrollerHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scrollingTMP;
    [SerializeField] private TextMeshProUGUI scrollingTMPClone;
    [SerializeField] private float scrollSpeed;
    private float width;
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
        }

        if (textRT != null)
            textRT.anchoredPosition = Vector2.MoveTowards(textRT.anchoredPosition, new Vector2(
                textRT.anchoredPosition.x - startPos.x, textRT.anchoredPosition.y), scrollSpeed * Time.deltaTime);
        if (cloneRT != null)
            cloneRT.anchoredPosition = Vector2.MoveTowards(cloneRT.anchoredPosition, new Vector2(
               cloneRT.anchoredPosition.x - startPos.x, cloneRT.anchoredPosition.y), scrollSpeed * Time.deltaTime);

        if (textRT.anchoredPosition.x <= width * -1 + startPos.x)
        {
            Destroy(scrollingTMP.gameObject);
            scrollingTMP = scrollingTMPClone;
            textRT = scrollingTMP.GetComponent<RectTransform>();
            scrollingTMPClone = null;
            SpawnNewText();
        }
    }
    public void SpawnNewText()
    {
        if (scrollingTMPClone != null)
            return;
        scrollingTMP.gameObject.name = "Text";
        float halfWidth = GetComponent<RectTransform>().sizeDelta.x/2;
        width = scrollingTMP.preferredWidth;
        scrollingTMPClone = Instantiate(scrollingTMP, transform);
        scrollingTMPClone.gameObject.name="Text Clone";
        cloneRT = scrollingTMPClone.GetComponent<RectTransform>();
        cloneRT.anchoredPosition = new Vector2(textRT.anchoredPosition.x + width + halfWidth, textRT.anchoredPosition.y);
    }
}
