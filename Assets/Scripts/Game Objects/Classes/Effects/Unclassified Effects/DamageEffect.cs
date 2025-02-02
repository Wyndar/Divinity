using System;

public class DamageEffect : IEffectStrategy
{
    private static readonly Lazy<DamageEffect> _instance = new(() => new DamageEffect());
    public static DamageEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        combatantLogic.TakeDamage(subEffect.EffectAmount, false);
    }
}
