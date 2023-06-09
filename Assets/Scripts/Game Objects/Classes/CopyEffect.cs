﻿using System;
public class CopyEffect:Effect
{
    public CopyEffect(Effect copyEffect)
    {
        EffectType = new(copyEffect.EffectType);
        EffectUsed = new(copyEffect.EffectUsed);
        EffectAmount = new(copyEffect.EffectAmount);
        EffectActivationIsMandatory = new(copyEffect.EffectActivationIsMandatory);

        ActivationLocation = copyEffect.ActivationLocation;
        MaxActivations = copyEffect.MaxActivations;

        if (copyEffect.TargetingType != null)
        {
            TargetingType = new(copyEffect.TargetingType);
            if (copyEffect.EffectTargetType != null)
                EffectTargetType = new(copyEffect.EffectTargetType);
            if (copyEffect.effectTargetPlayType != null)
                EffectTargetPlayType = new(copyEffect.EffectTargetPlayType);
            if (copyEffect.EffectTargetOwner != null)
                EffectTargetOwner = new(copyEffect.EffectTargetOwner);
            TargetLocation = new(copyEffect.TargetLocation);
            EffectTargetAmount = new(copyEffect.EffectTargetAmount);
            AllowSelfTarget = new(copyEffect.AllowSelfTarget);
        }
        if (copyEffect.TargetCountModifier != null)
            TargetCountModifier = new(copyEffect.TargetCountModifier);
        if (copyEffect.TargetStat != null)
        {
            TargetStat = new(copyEffect.TargetStat);
            TargetStatCondition = new(copyEffect.TargetStatCondition);
            TargetStatAmount = new(copyEffect.TargetStatAmount);
        }
        if (copyEffect.TriggerType != null)
            TriggerType = new(copyEffect.TriggerType);
        if (copyEffect.TriggerCard != null)
            TriggerCard = new(copyEffect.TriggerCard);
        if (copyEffect.TriggerInfo != null)
            TriggerInfo = new(copyEffect.TriggerInfo);
        if (copyEffect.triggerLocation != null)
            TriggerLocation = new(copyEffect.TriggerLocation);
        if (copyEffect.triggerState != null)
            TriggerState = new(copyEffect.TriggerState);
        if (copyEffect.TriggerPhase != null)
            TriggerPhase = new(copyEffect.TriggerPhase);
    }
}

