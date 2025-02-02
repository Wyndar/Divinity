using System;
using System.Linq;

public class BurnDetonateEffect : IEffectStrategy
{
    private static readonly Lazy<BurnDetonateEffect> _instance = new(() => new BurnDetonateEffect());
    public static BurnDetonateEffect Instance => _instance.Value;

    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        foreach (var status in combatantLogic.cardStatuses.OfType<Burn>())
        {
            status.DetonateActions(caster.gameManager);
        }
    }
}