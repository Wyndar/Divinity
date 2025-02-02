using System;

public class WeakenEffect : IEffectStrategy
{
    private static readonly Lazy<WeakenEffect> _instance = new(() => new WeakenEffect());
    public static WeakenEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.HpLoss);
        combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.AtkLoss);
    }
}
