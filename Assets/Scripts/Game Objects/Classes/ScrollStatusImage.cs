﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ScrollStatusImage : MonoBehaviour, IPointerClickHandler
{
    public CardStatus currentStatus;
    public Game_Manager Game_Manager;
	public UXManager UXManager;
    public FloatingText floatingText;
    public Image highlightImage;
    public Image image;
    public TMP_Text durationText;
    public bool isTargeted;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (Game_Manager.isChecking)
            return;
        
        if (isTargeted)
        {
            if (floatingText != null)
                floatingText.DisableFoatingText();
            highlightImage.color = Color.clear;
            isTargeted = false;
        }
        else
        {
            isTargeted = true;
            highlightImage.color = Color.yellow;
            floatingText = UXManager.EnableInfoTextPanel(currentStatus.name, currentStatus.description, currentStatus is Buff ? Color.green : Color.red, Color.black, this);
        }
    }
}