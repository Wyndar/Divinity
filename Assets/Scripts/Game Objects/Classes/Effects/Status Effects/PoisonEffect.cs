using System;

public class PoisonEffect : IEffectStrategy
{
    private static readonly Lazy<PoisonEffect> _instance = new(() => new PoisonEffect());
    public static PoisonEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var monsterLogic = target.GetComponent<MonsterLogic>();
        var status = new Poison(caster, target, subEffect.duration);
        combatantLogic.cardStatuses.Add(status);
        monsterLogic.currentSlot.SetStatusIcon(status);
    }
}