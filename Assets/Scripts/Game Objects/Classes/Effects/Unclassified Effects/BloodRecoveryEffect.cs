using System;

internal class BloodRecoveryEffect : IEffectStrategy
{
    private static readonly Lazy<BloodRecoveryEffect> _instance = new(() => new BloodRecoveryEffect());
    public static BloodRecoveryEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        caster.dataLogic.cardController.BloodGain(Attunement.Untuned, subEffect.EffectAmount);
    }
}