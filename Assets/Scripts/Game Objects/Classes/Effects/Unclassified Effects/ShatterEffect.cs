using System;

public class ShatterEffect : IEffectStrategy
{
    private static readonly Lazy<ShatterEffect> _instance = new(() => new ShatterEffect());
    public static ShatterEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var monsterLogic = target.GetComponent<MonsterLogic>();
        monsterLogic.MonsterDeath();
    }
}
