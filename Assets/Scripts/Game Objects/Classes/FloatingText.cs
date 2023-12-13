using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public UXManager UXmanager;
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
        UXmanager.DisableInfoPauseMode();
        Destroy(gameObject);
    }
}