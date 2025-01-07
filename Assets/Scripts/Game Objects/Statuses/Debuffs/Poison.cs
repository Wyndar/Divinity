using UnityEngine;
[System.Serializable]
public class Poison : Debuff
{
    public Poison(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Poison");
        debuff = Debuffs.Poisoned;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer > 0 ? timer : 4;
        shouldCountdown = true;
        canDetonate = true;
        sprite = uIManager.poisonSprite;
    }
    public override void TimerActions(GameBattleManager gm, GameObject timerPrefab)
    {
        base.TimerActions(gm, timerPrefab);
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.StatAdjustment(1, Status.HpLoss);
    }

    public override void DetonateActions(GameBattleManager gm)
    {
        if (Timer <= 0)
            return;
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.StatAdjustment(Timer, Status.HpLoss);
        gm.StateChange(GameState.Detonate);
    }
}
