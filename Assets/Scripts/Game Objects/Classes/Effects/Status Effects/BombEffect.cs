using System;

public class BombEffect : IEffectStrategy
{
    private static readonly Lazy<BombEffect> _instance = new(() => new BombEffect());
    public static BombEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var combatantLogic = target.GetComponent<CombatantLogic>();
        var monsterLogic = target.GetComponent<MonsterLogic>();
        var status = new Bomb(caster, target, subEffect.duration);
        combatantLogic.cardStatuses.Add(status);
        monsterLogic.currentSlot.SetStatusIcon(status);
    }
}
