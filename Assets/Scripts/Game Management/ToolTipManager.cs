using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipManager : MonoBehaviour
{
    public Game_Manager gm;

    public UXManager ux;

    public GameObject infoTextPanel, canvasGO;

    public List<ToolTipInfo> tooltipInfos;

    public int floatingInfoTextCount;
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
        GameObject floater = Instantiate(infoTextPanel, canvasGO.transform);
        gm.isShowingInfo = true;
        floatingInfoTextCount++;
        //trying to get it to appear on the opposite half of the screen to the clicked target
        Vector3 viewPos = Camera.main.WorldToViewportPoint(ux.touchEndPosition);
        floater.transform.position = new(viewPos.x < 0.5f ? ux.touchEndPosition.x + 1f : ux.touchEndPosition.x - 1f, viewPos.y > 0.5f ? ux.touchEndPosition.y - 4f : ux.touchEndPosition.y + 4f, floater.transform.position.z);
        FloatingText floatingText = floater.GetComponent<FloatingText>();
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
        floatingInfoTextCount--;
        if (floatingInfoTextCount < 1)
            gm.isShowingInfo = false;
    }
}