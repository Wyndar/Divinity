using System;
using System.Linq;

public class PoisonDetonateEffect : IEffectStrategy
{
    private static readonly Lazy<PoisonDetonateEffect> _instance = new(() => new PoisonDetonateEffect());
    public static PoisonDetonateEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        foreach (var status in combatantLogic.cardStatuses.OfType<Poison>())
        {
            status.DetonateActions(caster.gameManager);
        }
    }
}