using System;

public class BuffDispelEffect : IEffectStrategy
{
    private static readonly Lazy<BuffDispelEffect> _instance = new(() => new BuffDispelEffect());
    public static BuffDispelEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        for (int i = 0; i < subEffect.EffectAmount; i++)
        {
            var status = combatantLogic.BuffCheck(Buffs.Undefined);
            if (status != null)
            {
                combatantLogic.RemoveCardStatus(status);
            }
        }
    }
}
