using System;

public class VigorEffect : IEffectStrategy
{
    private static readonly Lazy<VigorEffect> _instance = new(() => new VigorEffect());
    public static VigorEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.AtkGain);
        combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.HpGain);
    }
}
