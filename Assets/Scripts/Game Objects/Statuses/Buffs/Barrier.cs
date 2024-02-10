using UnityEngine;

[System.Serializable]
public class Barrier : Buff
{
    public Barrier(CardLogic bufferLogic, CardLogic buffedLogic, int amount, int duration)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<UIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Barrier");
        buff = Buffs.Barrier;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        shouldCountdown = duration != 0;
        Timer = duration;
        Amount = amount;
        sprite = uIManager.barrierSprite;
    }
}
