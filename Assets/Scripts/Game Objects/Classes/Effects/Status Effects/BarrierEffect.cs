using System;

public class BarrierEffect : IEffectStrategy
{
    private static readonly Lazy<BarrierEffect> _instance = new(() => new BarrierEffect());
    public static BarrierEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var buff = new Barrier(caster, target, subEffect.EffectAmount, subEffect.duration);
        combatantLogic.AddNonStackingBuff(buff);
    }
}