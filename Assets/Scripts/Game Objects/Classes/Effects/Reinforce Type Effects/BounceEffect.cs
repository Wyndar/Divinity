using System;

public class BounceEffect : IEffectStrategy
{
    private static readonly Lazy<BounceEffect> _instance = new(() => new BounceEffect());
    public static BounceEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var monsterLogic = target.GetComponent<MonsterLogic>();
        target.StartCoroutine(monsterLogic.BounceCard());
    }
}