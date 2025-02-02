using System;

internal class GeneralEffect : IEffectStrategy
{
    private static readonly Lazy<GeneralEffect> _instance = new(() => new GeneralEffect());
    public static GeneralEffect Instance => _instance.Value;

    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        target.EffectHandler(subEffect); ;
    }
}