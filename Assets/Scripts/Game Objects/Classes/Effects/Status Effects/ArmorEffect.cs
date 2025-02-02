using System;

public class ArmorEffect : IEffectStrategy
{
    private static readonly Lazy<ArmorEffect> _instance = new(() => new ArmorEffect());
    public static ArmorEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var buff = new Armor(caster, target, subEffect.EffectAmount, subEffect.duration);
        combatantLogic.AddNonStackingBuff(buff);
    }
}