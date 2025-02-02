using System;

public class SilenceEffect : IEffectStrategy
{
    private static readonly Lazy<SilenceEffect> _instance = new(() => new SilenceEffect());
    public static SilenceEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var debuff = new Silence(caster, target, subEffect.duration);
        combatantLogic.AddNonStackingDebuff(debuff);
    }
}