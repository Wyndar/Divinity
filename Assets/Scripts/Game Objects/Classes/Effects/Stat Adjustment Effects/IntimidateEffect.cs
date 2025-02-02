using System;

public class IntimidateEffect : IEffectStrategy
{
    private static readonly Lazy<IntimidateEffect> _instance = new(() => new IntimidateEffect());
    public static IntimidateEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.AtkLoss);
    }
}
