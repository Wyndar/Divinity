using System;
using System.Linq;

public class BombDetonateEffect : IEffectStrategy
{
    private static readonly Lazy<BombDetonateEffect> _instance = new(() => new BombDetonateEffect());
    public static BombDetonateEffect Instance => _instance.Value;

    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        foreach (var status in combatantLogic.cardStatuses.OfType<Bomb>())
        {
            status.DetonateActions(caster.gameManager);
        }
    }
}