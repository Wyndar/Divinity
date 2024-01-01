﻿using UnityEngine;
using System;

[Serializable]
public class CardStatus
{
    public CardLogic affectedLogic;
    public CardLogic applierLogic;
    public int Timer;
    public int Amount = 0;
    public Sprite sprite;
    public bool shouldCountdown;
    public bool hasDoneCountDownThisTurn;
    public ToolTipInfo toolTipInfo;
    public FieldIconHolder fieldIconHolder;

    public CardStatus()
    {

    }

    public CardStatus(CardLogic bufferLogic, CardLogic buffedLogic, int timer, bool timed, Sprite bufSprite, ToolTipInfo toolTip)
    {
        affectedLogic = bufferLogic;
        applierLogic = buffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        sprite = bufSprite;
        toolTipInfo = toolTip;
    }
}