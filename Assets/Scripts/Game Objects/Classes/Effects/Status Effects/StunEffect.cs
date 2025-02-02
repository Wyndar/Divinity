using System;

public class StunEffect : IEffectStrategy
{
    private static readonly Lazy<StunEffect> _instance = new(() => new StunEffect());
    public static StunEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var debuff = new Stun(caster, target, subEffect.duration);
        combatantLogic.AddNonStackingDebuff(debuff);
    }
}