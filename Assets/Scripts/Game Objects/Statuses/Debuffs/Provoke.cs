using UnityEngine;
[System.Serializable]
public class Provoke : Debuff
{
    public Provoke(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        toolTipManager = GameObject.FindFirstObjectByType<ToolTipManager>();
        uIManager = GameObject.FindFirstObjectByType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Provoke");
        debuff = Debuffs.Provoked;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timer != 0;
        sprite = uIManager.provokeSprite;
    }
}
