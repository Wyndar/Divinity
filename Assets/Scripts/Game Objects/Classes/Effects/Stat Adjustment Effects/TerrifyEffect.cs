using System;

public class TerrifyEffect : IEffectStrategy
{
    private static readonly Lazy<TerrifyEffect> _instance = new(() => new TerrifyEffect());
    public static TerrifyEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.HpLoss);
    }
}