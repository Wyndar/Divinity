using System.Collections.Generic;

[System.Serializable]
public class SubEffect
{
    public EffectTypes effectType;
    public EffectsUsed effectUsed;
    public List<Location> targetLocations = new();
    public Controller effectTargetController;
    public TargetingTypes targetingType;
    public List<Type> effectTargetTypes = new();

    public int effectAmount;
    public int effectTargetAmount;

    public EffectTypes EffectType { get; set; }
    public EffectsUsed EffectUsed { get; set; }
    public int EffectAmount { get; set; }
    public bool EffectActivationIsMandatory { get; set; }

    //if needs targets
    public TargetingTypes TargetingType { get; set; }
    public List<Type> EffectTargetTypes { get; set; }
    public Controller EffectTargetController { get; set; }
    public List<Location> TargetLocations { get; set; }
    public int EffectTargetAmount { get; set; }
    public bool AllowSelfTarget { get; set; }

    //if should count target
    public float TargetCountModifier { get; set; }

    //if needs target stats
    public List<string> TargetStats { get; set; }
    public List<string> TargetStatConditions { get; set; }
    public List<int> TargetStatAmounts { get; set; }
}