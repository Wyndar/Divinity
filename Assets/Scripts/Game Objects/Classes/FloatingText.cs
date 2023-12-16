using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public ToolTipManager toolTipManager;
    public TMP_Text header;
    public TextMeshProUGUI infoText;
    public ScrollStatusImage scrollStatusImage;

    public void DisableFoatingText()
    {
        if (scrollStatusImage != null)
        {
            scrollStatusImage.highlightImage.color = Color.clear;
            scrollStatusImage.isTargeted = false;
        }
        toolTipManager.DisableInfoPauseMode();
        Destroy(gameObject);
    }
}