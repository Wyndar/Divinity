using UnityEngine;
[System.Serializable]
public class Sleep : Debuff
{
    public Sleep(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Sleep");
        debuff = Debuffs.Stunned;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timer != 0;
        sprite = uIManager.sleepSprite;
    }
}
