using System;

public class StealthEffect : IEffectStrategy
{
    private static readonly Lazy<StealthEffect> _instance = new(() => new StealthEffect());
    public static StealthEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var status = new Stealth(caster, target, subEffect.duration);
        combatantLogic.SetTargetStatus(status, TargetState.Stealth);
    }
}
