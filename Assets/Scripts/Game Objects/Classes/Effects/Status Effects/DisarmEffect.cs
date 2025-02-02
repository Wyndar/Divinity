using System;

public class DisarmEffect : IEffectStrategy
{
    private static readonly Lazy<DisarmEffect> _instance = new(() => new DisarmEffect());
    public static DisarmEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var debuff = new Disarm(caster, target, subEffect.duration);
        combatantLogic.AddNonStackingDebuff(debuff);
    }
}