﻿using System;
using System.Collections.Generic;

[System.Serializable]
public class SubEffect
{
    //this is just a reference, we don't want to break the code with some weird infinite recursion now, do we?
    [NonSerialized] public Effect parentEffect;
    public EffectTypes effectType;
    public EffectsUsed effectUsed;
    public List<Location> targetLocations = new();
    public Controller effectTargetController;
    public TargetingTypes targetingType;
    public List<Type> effectTargetTypes = new();

    public int effectAmount;
    public int effectTargetAmount;

    public EffectTypes EffectType { get; private set; }
    public EffectsUsed EffectUsed { get; private set; }
    public int EffectAmount { get; private set; }
    public bool EffectActivationIsMandatory { get; private set; }

    //if needs targets
    public TargetingTypes TargetingType { get; private set; }
    public List<Type> EffectTargetTypes { get; private set; }
    public Controller EffectTargetController { get; private set; }
    public List<Location> TargetLocations { get; private set; }
    public int EffectTargetAmount { get; private set; }
    public bool AllowSelfTarget { get; private set; }

    //if should count target
    public float TargetCountModifier { get; private set; }

    //if needs target stats
    public List<string> TargetStats { get; private set; }
    public List<string> TargetStatConditions { get; private set; }
    public List<int> TargetStatAmounts { get; private set; }

    public void SetDataFromInfo()
    {
        EffectType = effectType;
        EffectUsed = effectUsed;
        EffectAmount = effectAmount;
        TargetingType = targetingType;
        EffectTargetTypes = effectTargetTypes;
        EffectTargetAmount = effectTargetAmount;
        TargetLocations = targetLocations;
        EffectTargetController = effectTargetController;
    }
}