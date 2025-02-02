using System;

public class BurnEffect : IEffectStrategy
{
    private static readonly Lazy<BurnEffect> _instance = new(() => new BurnEffect());
    public static BurnEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var monsterLogic = target.GetComponent<MonsterLogic>();
        var status = new Burn(caster, target, subEffect.duration);
        combatantLogic.cardStatuses.Add(status);
        monsterLogic.currentSlot.SetStatusIcon(status);
    }
}