using UnityEngine;
[System.Serializable]
public class Poison : Debuff
{
    public Poison(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<UIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Poison");
        debuff = Debuffs.Poisoned;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer > 0 ? timer : 4;
        shouldCountdown = true;
        canDetonate = true;
        sprite = uIManager.poisonSprite;
    }
    public override void TimerActions(Game_Manager gm, GameObject timerPrefab)
    {
        base.TimerActions(gm, timerPrefab);
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.StatAdjustment(1, Status.HpLoss);
    }

    public override void DetonateActions(Game_Manager gm)
    {
        if (Timer <= 0)
            return;
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.StatAdjustment(Timer, Status.HpLoss);
        gm.StateChange(GameState.Detonate);

        //unnecesary bloat just so broken chain execution in detonate effects work
        if (gm.currentFocusCardLogic != null && gm.isActivatingEffect && !gm.isWaitingForResponse && gm.gameState == GameState.Detonate)
            gm.currentFocusCardLogic.FinishResolution(gm.currentFocusCardLogic.focusSubEffect);
    }
}
