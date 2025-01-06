using System.Collections.Generic;
using UnityEngine;

public class ToolTipManager : MonoBehaviour
{
    public Game_Manager gm;

    public MainUIManager ux;

    public GameObject infoTextPanel, canvasGO;

    public List<ToolTipInfo> tooltipInfos;

    public List<FloatingText> floatingInfoTexts;
    public void OnEnable() => ClickableText.OnLinkClickEvent += GetToolTipInfo;
    public void OnDisable() => ClickableText.OnLinkClickEvent -= GetToolTipInfo;

    private void GetToolTipInfo(string eventKey)
    {
        
        foreach (ToolTipInfo tooltipInfo in tooltipInfos)
        {
            if (tooltipInfo.key == eventKey)
            {
                EnableInfoTextPanel(tooltipInfo, Color.black, Color.black, null);
                return;
            }
        }
        Debug.Log($"Keyword: {eventKey} not found");
    }

    public FloatingText EnableInfoTextPanel(ToolTipInfo toolTip, Color headerColor, Color infoColor, ScrollStatusImage scrollStatusImage)
    {
        if (floatingInfoTexts.Find(a => a.infoText.text == toolTip.value) != null)
            return null;
        GameObject floater = Instantiate(infoTextPanel, canvasGO.transform);
        gm.isShowingInfo = true;
        //trying to get it to appear on the opposite half of the screen to the clicked target
        Vector3 viewPos = Camera.main.WorldToViewportPoint(ux.touchEndPosition);
        //inverted because of landscape
        floater.transform.position = new(viewPos.y > 0.5f ? ux.touchEndPosition.y - 4f : ux.touchEndPosition.y + 4f,
            viewPos.x < 0.5f ? ux.touchEndPosition.x + 1 : ux.touchEndPosition.x - 1f, floater.transform.position.z);
        FloatingText floatingText = floater.GetComponent<FloatingText>();
        floatingInfoTexts.Add(floatingText);
        floatingText.header.text = toolTip.key;
        floatingText.infoText.text = toolTip.value;
        floatingText.header.color = headerColor;
        floatingText.infoText.color = infoColor;
        floatingText.toolTipManager = this;
        floatingText.scrollStatusImage = scrollStatusImage;

        return floatingText;
    }

    public void DisableInfoPauseMode()
    {
        floatingInfoTexts[^1].DisableFloatingText();
        if (floatingInfoTexts.Count < 1)
            gm.isShowingInfo = false;
    }
}