using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ScrollStatusImage : MonoBehaviour, IPointerClickHandler
{
    public CardStatus currentStatus;
    public GameBattleManager Game_Manager;
	public ToolTipManager toolTipManager;
    public FloatingText floatingText;
    public Image highlightImage;
    public Image image;
    public TMP_Text durationText;
    public TMP_Text amountText;
    public bool isTargeted;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Game_Manager.isChecking)
            return;
        
        if (isTargeted)
        {
            if (floatingText != null)
               toolTipManager.DisableInfoPauseMode();
            highlightImage.color = Color.clear;
            isTargeted = false;
        }
        else
        {
            isTargeted = true;
            highlightImage.color = Color.yellow;
            floatingText = toolTipManager.EnableInfoTextPanel(currentStatus.toolTipInfo, currentStatus is Buff ? Color.blue : Color.red, Color.black, this);
        }
    }
}