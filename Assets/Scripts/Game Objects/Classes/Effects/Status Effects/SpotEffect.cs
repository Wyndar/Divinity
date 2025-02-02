using System;

public class SpotEffect : IEffectStrategy
{
    private static readonly Lazy<SpotEffect> _instance = new(() => new SpotEffect());
    public static SpotEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var debuff = new Spot(caster, target, subEffect.duration);
        combatantLogic.AddNonStackingDebuff(debuff);
    }
}
