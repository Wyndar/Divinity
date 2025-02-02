using System;

public class DebuffDispelEffect : IEffectStrategy
{
    private static readonly Lazy<DebuffDispelEffect> _instance = new(() => new DebuffDispelEffect());
    public static DebuffDispelEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        for (int i = 0; i < subEffect.EffectAmount; i++)
        {
            var status = combatantLogic.DebuffCheck(Debuffs.Undefined);
            if (status != null)
            {
                combatantLogic.RemoveCardStatus(status);
            }
        }
    }
}