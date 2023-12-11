using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollCardImage : MonoBehaviour, IPointerClickHandler
{
	public CardLogic cardLogic;
	public Game_Manager Game_Manager;
    public Image highlightImage;
    public Image image;
    private bool isTargeted;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Game_Manager.isChecking)
        {
            Game_Manager.DisableEffectInfoPanels();
            Game_Manager.currentFocusCardLogic = cardLogic;
            Game_Manager.ShowEffectInfoPanel();
            return;
        }

        int effectNum = Game_Manager.currentFocusCardLogic.effectCountNumber;
        int subEffectNum = Game_Manager.currentFocusCardLogic.subCountNumber;
        
        if (isTargeted)
        {
            highlightImage.color = Color.clear;
            isTargeted = false;
            cardLogic.ManualTargetRemoval();
        }
        else
        {
            isTargeted = true;
            highlightImage.color = Color.yellow;
            cardLogic.ManualTargetAcquisition(effectNum, subEffectNum);
        }
    }
}

