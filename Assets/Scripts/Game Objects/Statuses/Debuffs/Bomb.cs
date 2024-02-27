using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Bomb : Debuff
{
    public Bomb(CardLogic debufferLogic, CardLogic debuffedLogic, int timer)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<UIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Bomb");
        debuff = Debuffs.Bombed;
        applierLogic = debufferLogic;
        affectedLogic = debuffedLogic;
        Timer = timer > 0 ? timer : 3;
        shouldCountdown = true;
        canDetonate = true;
        sprite = uIManager.bombSprite;
    }

    public override void DetonateActions(Game_Manager gm)
    {
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        combatant.TakeDamage(3, false);
        gm.StateChange(GameState.Detonate);
        if (affectedLogic.currentLocation == Location.Grave)
            return;
        Stun stun = new(applierLogic, affectedLogic, 3);
        combatant.cardStatuses.Add(stun);
        CardStatusHistoryEntry cardStatusHistoryEntry = new(stun)
        {
            loggedCard = affectedLogic,
            loggedLocation = affectedLogic.currentLocation,
            logIndex = gm.gameLogHistoryEntries.Count
        };
        gm.gameLogHistoryEntries.Add(cardStatusHistoryEntry);
        affectedLogic.cardController.SetStatusIcon(affectedLogic.locationOrderNumber, stun);
        gm.StateChange(GameState.Stun);
        if (fieldIconHolder != null)
        {
            fieldIconHolder.transform.SetParent(null);
            fieldIconHolder.DestroyThis();
        }
        return;
    }
}
