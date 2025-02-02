using System;

public class SleepEffect : IEffectStrategy
{
    private static readonly Lazy<SleepEffect> _instance = new(() => new SleepEffect());
    public static SleepEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var debuff = new Sleep(caster, target, subEffect.duration);
        combatantLogic.AddNonStackingDebuff(debuff);
    }
}