using System;

public class CamouflageEffect : IEffectStrategy
{
    private static readonly Lazy<CamouflageEffect> _instance = new(() => new CamouflageEffect());
    public static CamouflageEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var status = new Camouflage(caster, target, subEffect.duration);
        combatantLogic.SetTargetStatus(status, TargetState.Stealth);
    }
}