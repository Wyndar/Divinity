using System.Collections.Generic;
using System;

internal class BloodCostEffect : IEffectStrategy
{
    private static readonly Lazy<BloodCostEffect> _instance = new(() => new BloodCostEffect());
    public static BloodCostEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        var attunement = Enum.Parse<Attunement>(subEffect.TargetStats[0]);
        caster.dataLogic.cardController.BloodLoss(new List<Attunement> { attunement }, subEffect.EffectAmount);
    }
}