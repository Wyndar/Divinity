using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ClickableText : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI text;

    public delegate void LinkClickEvent(string eventKey);
    public static event LinkClickEvent OnLinkClickEvent;
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        Vector3 mousePosition = new(pointerEventData.position.x, pointerEventData.position.y, 0);

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, mousePosition, Camera.main);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            OnLinkClickEvent?.Invoke(linkInfo.GetLinkText());
        }
    }
}