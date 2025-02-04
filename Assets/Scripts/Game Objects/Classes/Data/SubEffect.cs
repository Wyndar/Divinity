using System;
using System.Collections.Generic;

[Serializable]
public class SubEffect
{
    // Fixed values from JSON loading
    public EffectsUsed EffectUsed { get; private set; }
    public EffectTypes EffectType { get; private set; }
    public int EffectAmount { get; private set; }
    public bool EffectActivationIsMandatory { get; private set; }
    public int Duration { get; private set; }
    public List<Location> TargetLocations { get; private set; }
    public Controller EffectTargetController { get; private set; }
    public TargetingTypes TargetingType { get; private set; }
    public List<Type> EffectTargetTypes { get; private set; }
    public int EffectTargetAmount { get; private set; }
    public bool AllowSelfTarget { get; private set; }
    public List<string> TargetStats { get; private set; }
    public List<string> TargetStatConditions { get; private set; }
    public List<int> TargetStatAmounts { get; private set; }
    public int TargetCountModifier { get; private set; }
    public List<int> DependentIndices { get; private set; }
    public List<DependentEffectParameter> DependentEffectParameters { get; private set; }
    [NonSerialized]
    public Effect ParentEffect;

    // Editable values for runtime modification
    public EffectsUsed effectUsed;
    public EffectTypes effectType;
    public int duration;
    public int effectAmount;
    public List<Location> targetLocations;
    public Controller effectTargetController;
    public TargetingTypes targetingType;
    public List<Type> effectTargetTypes;
    public int effectTargetAmount;
    public bool allowSelfTarget;
    public List<string> targetStats;
    public List<string> targetStatConditions;
    public List<int> targetStatAmounts;
    public List<int> dependentIndices;
    public List<DependentEffectParameter> dependentEffectParameters;

    public SubEffect(EffectsUsed effectUsed, int effectAmount, int duration, Effect parentEffect, List<Location> targetLocations = null,
        Controller effectTargetController = default, TargetingTypes targetingType = default, List<Type> effectTargetTypes = null,
        int effectTargetAmount = 0, bool allowSelfTarget = false, List<string> targetStats = null, List<string> targetStatConditions = null,
        List<int> targetStatAmounts = null, List<int> dependentIndices = null, List<DependentEffectParameter> dependentEffectParameters = null)
    {
        // Assign fixed values
        EffectUsed = effectUsed;
        Duration = duration;
        TargetLocations = targetLocations ?? new List<Location>();
        EffectTargetController = effectTargetController;
        TargetingType = targetingType;
        EffectTargetTypes = effectTargetTypes ?? new List<Type>();
        EffectTargetAmount = effectTargetAmount;
        AllowSelfTarget = allowSelfTarget;
        TargetStats = targetStats ?? new List<string>();
        TargetStatConditions = targetStatConditions ?? new List<string>();
        TargetStatAmounts = targetStatAmounts ?? new List<int>();
        DependentIndices = dependentIndices ?? new List<int>();
        DependentEffectParameters = dependentEffectParameters ?? new List<DependentEffectParameter>();
        this.ParentEffect = parentEffect;

        // Assign editable values
        this.effectAmount = effectAmount;
        this.duration = duration;
        this.targetLocations = new List<Location>(TargetLocations);
        this.effectTargetController = EffectTargetController;
        this.targetingType = TargetingType;
        this.effectTargetTypes = new List<Type>(EffectTargetTypes);
        this.effectTargetAmount = EffectTargetAmount;
        this.allowSelfTarget = AllowSelfTarget;
        this.targetStats = new List<string>(TargetStats);
        this.targetStatConditions = new List<string>(TargetStatConditions);
        this.targetStatAmounts = new List<int>(TargetStatAmounts);
        this.dependentIndices = new List<int>(DependentIndices);
        this.dependentEffectParameters = new List<DependentEffectParameter>(DependentEffectParameters);
    }

    //empty constructor for newtonsoft to hide the internal one for our deckbuilder
    public SubEffect()
    {

    }
    internal SubEffect(SubEffect subEffect, Effect parent)
    {
        EffectType = subEffect.EffectType;
        EffectUsed = subEffect.EffectUsed;
        EffectAmount = subEffect.EffectAmount;
        EffectActivationIsMandatory = subEffect.EffectActivationIsMandatory;
        Duration = subEffect.Duration;
        TargetingType = subEffect.TargetingType;
        EffectTargetTypes = subEffect.EffectTargetTypes;
        EffectTargetController = subEffect.EffectTargetController;
        TargetLocations = subEffect.TargetLocations;
        EffectTargetAmount = subEffect.EffectTargetAmount;
        AllowSelfTarget = subEffect.AllowSelfTarget;
        TargetCountModifier = subEffect.TargetCountModifier;
        TargetStats = subEffect.TargetStats;
        TargetStatConditions = subEffect.TargetStatConditions;
        TargetStatAmounts = subEffect.TargetStatAmounts;
        duration = subEffect.Duration;
        ParentEffect = parent;
        effectAmount = subEffect.EffectAmount;
        effectTargetAmount = subEffect.EffectTargetAmount;
        effectUsed = subEffect.EffectUsed;
        effectType = subEffect.EffectType;
        effectTargetController = subEffect.EffectTargetController;
        if (subEffect.DependentIndices != null)
        {
            DependentIndices = new(subEffect.DependentIndices);
            DependentEffectParameters = new(subEffect.DependentEffectParameters);
        }
        targetingType = subEffect.TargetingType;
        if (subEffect.TargetLocations != null)
            targetLocations = new(subEffect.TargetLocations);
        if (subEffect.EffectTargetTypes != null)
            effectTargetTypes = new(subEffect.EffectTargetTypes);
    }
}