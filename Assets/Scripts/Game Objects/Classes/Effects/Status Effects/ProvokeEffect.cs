using System;

public class ProvokeEffect : IEffectStrategy
{
    private static readonly Lazy<ProvokeEffect> _instance = new(() => new ProvokeEffect());
    public static ProvokeEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var status = new Provoke(caster, target, subEffect.duration);
        combatantLogic.SetTargetStatus(status, TargetState.Stealth);
    }
}