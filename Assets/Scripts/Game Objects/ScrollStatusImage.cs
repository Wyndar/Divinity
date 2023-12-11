using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ScrollStatusImage : MonoBehaviour, IPointerClickHandler
{
	public CardLogic cardLogic;
    public CardStatus currentStatus;
    public bool isBuff;
	public Game_Manager Game_Manager;
    public Image highlightImage;
    public Image image;
    public TMP_Text durationText;
    private bool isTargeted;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Game_Manager.isChecking)
        {
            return;
        }
        
        if (isTargeted)
        {
            highlightImage.color = Color.clear;
            isTargeted = false;
        }
        else
        {
            isTargeted = true;
            highlightImage.color = Color.yellow;
        }
    }
}

