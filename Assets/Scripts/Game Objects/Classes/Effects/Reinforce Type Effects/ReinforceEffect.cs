using System;

internal class ReinforceEffect : IEffectStrategy
{
    private static readonly Lazy<ReinforceEffect> _instance = new(() => new ReinforceEffect());
    public static ReinforceEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target) 
        => caster.gameManager.StartCoroutine(caster.gameManager.DrawCard(subEffect.EffectAmount, caster.dataLogic.cardController));
}