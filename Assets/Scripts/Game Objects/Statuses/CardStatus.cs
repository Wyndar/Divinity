using UnityEngine;
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
    public bool canDetonate;
    public ToolTipInfo toolTipInfo;
    public FieldIconHolder fieldIconHolder;
    public ToolTipManager toolTipManager;
    public SecondaryUIManager uIManager;

    //empty constructor for flexible inheritance, do not delete... vs might show 0 refs but it's important
    public CardStatus()
    {

    }
    virtual public void TimerActions(Game_Manager gm, GameObject timerPrefab)
    {
        if (!shouldCountdown)
        {
            hasDoneCountDownThisTurn = true;
            return;
        }
        GameObject.Instantiate(timerPrefab, affectedLogic.GetComponent<MonsterLogic>().currentSlot.fieldIcon.transform);
        Timer--;
        hasDoneCountDownThisTurn = true;
        if (fieldIconHolder != null)
        {
            fieldIconHolder.durationText.text = Timer.ToString();
            if (Timer <= 0)
            {
                fieldIconHolder.transform.SetParent(null);
                fieldIconHolder.DestroyThis();
            }
        }
    }
    virtual public void DetonateActions(Game_Manager gm)
    {
        if (!canDetonate)
            return;
        Debug.Log("Failed Detonate Actions Override");
    }
}
