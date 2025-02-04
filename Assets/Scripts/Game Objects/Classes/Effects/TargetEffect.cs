using System;
using UnityEngine;

internal class TargetEffect : IEffectStrategy
{
    private static readonly Lazy<TargetEffect> _instance = new(() => new TargetEffect());
    public static TargetEffect Instance => _instance.Value;

    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        int subEffectIndex = subEffect.ParentEffect.SubEffects.FindIndex(a => a == subEffect);
        //use big E cos small e is supposed to be editable and change
        foreach (SubEffect sub in subEffect.ParentEffect.SubEffects)
        {
            int subIndex = subEffect.ParentEffect.SubEffects.FindIndex(a => a == sub);
            if (sub.DependentEffectParameters == null || subIndex <= subEffectIndex)
                continue;
            float mod = sub.TargetCountModifier > 0 ? sub.TargetCountModifier : 1;
            foreach (DependentEffectParameter dependent in sub.DependentEffectParameters)
            {
                int targetVariableIndex = sub.DependentEffectParameters.FindIndex(a => a == dependent);
                if (targetVariableIndex != -1 && sub.DependentIndices[targetVariableIndex] == subEffectIndex)
                    switch (dependent)
                    {
                        case DependentEffectParameter.EffectTargetAmount:
                            sub.effectTargetAmount = subEffect.TargetStats == null
                            ? Mathf.FloorToInt(caster.targetingLogic.targets.Count * mod)
                            : GetModifiedDepenndentParameterValue(caster, subEffect.TargetStats[targetVariableIndex], mod);
                            break;
                        case DependentEffectParameter.EffectAmount:
                            sub.effectAmount = subEffect.TargetStats == null
                            ? Mathf.FloorToInt(caster.targetingLogic.targets.Count * mod)
                            : GetModifiedDepenndentParameterValue(caster, subEffect.TargetStats[targetVariableIndex], mod);
                            break;
                        default:
                            throw new MissingReferenceException("Unimplemented dependent parameter required for effect");
                    }
            }
        }
    }
    private int GetModifiedDepenndentParameterValue(CardLogic cardLogic, string checkedStat, float mod)
    {
        cardLogic.targetingLogic.targets[0].TryGetComponent<CombatantLogic>(out var combatant);
        cardLogic.targetingLogic.targets[0].TryGetComponent<PlayableLogic>(out var playable);
        return checkedStat switch
        {
            "current atk" => Mathf.CeilToInt(combatant.currentAtk * mod),
            "cost" => Mathf.CeilToInt(playable.cost * mod),
            _ => throw new MissingReferenceException("unimplemented target stat"),
        };
    }
}