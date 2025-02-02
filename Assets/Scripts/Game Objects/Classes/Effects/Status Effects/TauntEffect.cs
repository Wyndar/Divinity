using System;

public class TauntEffect : IEffectStrategy
{
    private static readonly Lazy<TauntEffect> _instance = new(() => new TauntEffect());
    public static TauntEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var status = new Taunt(caster, target, subEffect.duration);
        combatantLogic.SetTargetStatus(status, TargetState.Taunt);
    }
}