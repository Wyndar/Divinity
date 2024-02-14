using UnityEngine;
[System.Serializable]
public class Silence : Debuff
{
    public Silence(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<UIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Silence");
        debuff = Debuffs.Stunned;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timer != 0;
        sprite = uIManager.silenceSprite;
    }
}
