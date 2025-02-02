using System;

public class RegenerationEffect : IEffectStrategy
{
    private static readonly Lazy<RegenerationEffect> _instance = new(() => new RegenerationEffect());
    public static RegenerationEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        combatantLogic.Heal(subEffect.EffectAmount);
    }
}
