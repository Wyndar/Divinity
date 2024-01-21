using UnityEngine;
[System.Serializable]
public class Bomb : Debuff
{
    public ToolTipManager ToolTipManager;
    public UIManager UIManager;
    public Debuffs debuffApplied = Debuffs.Bombed;
    public ToolTipInfo toolTip; 

    public Bomb(CardLogic debufferLogic, CardLogic debuffedLogic, int timer, bool timed)
    {
        ToolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        UIManager = GameObject.FindObjectOfType<UIManager>();
        toolTip = ToolTipManager.tooltipInfos.Find(a => a.key == "Bomb");
        debuff = debuffApplied;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer;
        shouldCountdown = timed;
        canDetonate = true;
        toolTipInfo = toolTip;
        sprite = UIManager.bombSprite;
    }

    public override void DetonateActions(Game_Manager gm)
    {
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.TakeDamage(3, false);
        gm.StateChange(GameState.Detonate);
        if (affectedLogic.currentLocation == Location.Grave)
            return;
        Stun stun = new(applierLogic, affectedLogic, 3, true);
        combatant.cardStatuses.Add(stun);
        affectedLogic.EffectLogger(EffectsUsed.Stun, 1, LogType.Debuff, null);
        affectedLogic.cardController.SetStatusIcon(affectedLogic.locationOrderNumber, stun);
        gm.StateChange(GameState.Stun);
        return;
    }
}
