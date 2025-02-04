using UnityEngine;
using System;

[Serializable]
public class StatusEffect : CardStatus, IEffectStrategy
{
    public StatusType statusType;
    public int effectAmount;
    public Sprite effectSprite;

    public StatusEffect(CardLogic applier, CardLogic target, StatusType type, int amount, int duration, Sprite sprite)
    {
        toolTipManager = GameObject.FindFirstObjectByType<ToolTipManager>();
        uIManager = GameObject.FindFirstObjectByType<SecondaryUIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == type.ToString());

        statusType = type;
        applierLogic = applier;
        affectedLogic = target;
        shouldCountdown = duration > 0;
        Timer = duration;
        effectAmount = amount;
        effectSprite = sprite;
        sprite = effectSprite;
    }

    public override void TimerActions(GameBattleManager gm, GameObject timerPrefab)
    {
        base.TimerActions(gm, timerPrefab);
        if (statusType == StatusType.Burn || statusType == StatusType.Poison)
        {
            affectedLogic.GetComponent<CombatantLogic>().TakeDamage(effectAmount, false);
        }
        else if (statusType == StatusType.Sleep || statusType == StatusType.Disarm)
        {
            Timer--;
            if (Timer <= 0)
            {
                affectedLogic.GetComponent<CombatantLogic>().cardStatuses.Remove(this);
            }
        }
    }
    public override void DetonateActions(GameBattleManager gm)
    {
        if (statusType == StatusType.Burn || statusType == StatusType.Poison)
        {
            affectedLogic.GetComponent<CombatantLogic>().TakeDamage(Timer * effectAmount, false);
            gm.StateChange(GameState.Detonate);
        }
        else if (statusType == StatusType.Bomb)
        {
            affectedLogic.GetComponent<CombatantLogic>().TakeDamage(3, false);
            gm.StateChange(GameState.Detonate);

            if (affectedLogic.dataLogic.currentLocation == Location.Grave)
                return;

            gm.QueueEffect(applierLogic, new SubEffect(EffectsUsed.Stun, 0, 3, new Effect()));

            gm.StateChange(GameState.Stun);

            if (fieldIconHolder != null)
            {
                fieldIconHolder.transform.SetParent(null);
                fieldIconHolder.DestroyThis();
            }
        }
    }

    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        StatusEffect newEffect = StatusEffectFactory.CreateStatusEffect(caster, target, statusType, subEffect.EffectAmount, subEffect.Duration);
        target.GetComponent<CombatantLogic>().cardStatuses.Add(newEffect);
        target.GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(newEffect);

        CardStatusHistoryEntry cardStatusHistoryEntry = new(newEffect)
        {
            loggedCard = target,
            loggedLocation = target.dataLogic.currentLocation,
            logIndex = caster.gameManager.gameLogHistoryEntries.Count
        };
        caster.gameManager.gameLogHistoryEntries.Add(cardStatusHistoryEntry);
    }
}

public static class StatusEffectFactory
{
    public static StatusEffect CreateStatusEffect(CardLogic caster, CardLogic target, StatusType type, int amount, int duration)
    {
        var uIManager = GameObject.FindFirstObjectByType<SecondaryUIManager>();
        Sprite effectSprite = type switch
        {
            StatusType.Armor => uIManager.armorSprite,
            StatusType.Barrier => uIManager.barrierSprite,
            StatusType.Burn => uIManager.burnSprite,
            StatusType.Poison => uIManager.poisonSprite,
            StatusType.Stun => uIManager.stunSprite,
            StatusType.Taunt => uIManager.tauntSprite,
            StatusType.Silence => uIManager.silenceSprite,
            StatusType.Spot => uIManager.spotSprite,
            StatusType.Provoke => uIManager.provokeSprite,
            StatusType.Bomb => uIManager.bombSprite,
            StatusType.Sleep => uIManager.sleepSprite,
            StatusType.Disarm => uIManager.disarmSprite,
            _ => throw new ArgumentException("Unknown status type")
        };

        return new StatusEffect(caster, target, type, amount, duration, effectSprite);
    }
}
