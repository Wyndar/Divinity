using System;

internal class RecruitEffect : IEffectStrategy
{
    private static readonly Lazy<RecruitEffect> _instance = new(() => new RecruitEffect());
    public static RecruitEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        caster.gameManager.StartSearchCard(target, caster);
    }
}