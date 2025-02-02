using System;
using System.Linq;

public class DetonateEffect : IEffectStrategy
{
    private static readonly Lazy<DetonateEffect> _instance = new(() => new DetonateEffect());
    public static DetonateEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        foreach (var status in combatantLogic.cardStatuses.ToList())
        {
            status.DetonateActions(caster.gameManager);
        }
    }
}
