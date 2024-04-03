using System.Collections.Generic;

public class SubEffect
{
    public EffectTypes effectType;
    public EffectsUsed effectUsed;
    public List<Location> targetLocations = new();
    public Controller effectTargetController;
    public TargetingTypes targetingType;
    public List<Type> effectTargetTypes = new();
    public List<PlayType> effectTargetPlayTypes = new();

    public int effectAmount;
    public int effectTargetAmount;

    public string EffectType { get; set; }
    public string EffectUsed { get; set; }
    public int EffectAmount { get; set; }
    public bool EffectActivationIsMandatory { get; set; }

    //if needs targets
    public string TargetingType { get; set; }
    public List<string> EffectTargetTypes { get; set; }
    public List<string> EffectTargetPlayTypes { get; set; }
    public string EffectTargetController { get; set; }
    public List<string> TargetLocations { get; set; }
    public int EffectTargetAmount { get; set; }
    public bool AllowSelfTarget { get; set; }

    //if should count target
    public float TargetCountModifier { get; set; }

    //if needs target stats
    public List<string> TargetStats { get; set; }
    public List<string> TargetStatConditions { get; set; }
    public List<int> TargetStatAmounts { get; set; }
}