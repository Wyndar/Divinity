using UnityEngine;
[System.Serializable]
public class Burn : Debuff
{
    public Burn(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        toolTipManager = GameObject.FindFirstObjectByType<ToolTipManager>();
        uIManager = GameObject.FindFirstObjectByType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Burn");
        debuff = Debuffs.Burned;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer > 0 ? timer : 2;
        shouldCountdown = true;
        canDetonate = true;
        sprite = uIManager.burnSprite;
    }
    public override void TimerActions(GameBattleManager gm, GameObject timerPrefab)
    {
        base.TimerActions(gm, timerPrefab);
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.TakeDamage(2, false);
    }

    public override void DetonateActions(GameBattleManager gm)
    {
        if (Timer <= 0)
            return;
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.TakeDamage(Timer * 2, false);
        gm.StateChange(GameState.Detonate);
    }
}
