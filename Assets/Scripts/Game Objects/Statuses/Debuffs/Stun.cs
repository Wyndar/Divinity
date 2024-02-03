using UnityEngine;
[System.Serializable]
public class Stun : Debuff
{
    public Stun(CardLogic debufferLogic, CardLogic debuffedLogic, int timer, bool timed)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<UIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Stun");
        debuff = Debuffs.Stunned;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        sprite = uIManager.stunSprite;
    }
}
