﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollCardImage : MonoBehaviour, IPointerClickHandler
{
    public CardLogic cardLogic;
	public GameBattleManager Game_Manager;
    public Image highlightImage;
    public Image image;
    private bool isTargeted;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Game_Manager.isChecking)
        {
            Game_Manager.DisableEffectInfoPanels();
            cardLogic.SetFocusCardLogic();

            //private knowledge gtfo
            if (cardLogic.isFaceDown)
                return;

            Game_Manager.ShowEffectInfoPanel();
            if (Game_Manager.isShowingLog)
                Game_Manager.DisableLog();
            return;
        }
        
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
            cardLogic.ManualTargetAcquisition(Game_Manager.currentFocusCardLogic.focusSubEffect);
        }
    }
}

