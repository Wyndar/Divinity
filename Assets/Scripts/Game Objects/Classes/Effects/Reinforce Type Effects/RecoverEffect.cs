using System;

public class RecoverEffect : IEffectStrategy
{
    private static readonly Lazy<RecoverEffect> _instance = new(() => new RecoverEffect());
    public static RecoverEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target) 
        => caster.gameManager.StartCoroutine(caster.gameManager.RecoverCard(target));
}