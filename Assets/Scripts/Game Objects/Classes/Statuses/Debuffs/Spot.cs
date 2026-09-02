using UnityEngine;
[System.Serializable]
public class Spot : Debuff
{
    public Spot(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        toolTipManager = GameObject.FindAnyObjectByType<ToolTipManager>();
        uIManager = GameObject.FindAnyObjectByType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Spot");
        debuff = Debuffs.Provoked;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timer != 0;
        sprite = uIManager.spotSprite;
    }
}
