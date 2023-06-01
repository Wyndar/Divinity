using System.Collections.Generic;

[System.Serializable]
public class Effect
{
    public List<string> EffectType { get; set; }

    public List<string> EffectUsed { get; set; }
    public List<int> EffectAmount { get; set; }
    public List<bool> EffectActivationIsMandatory { get; set; }

    //if can be activated 
    public string ActivationLocation { get; set; }
    public int MaxActivations { get; set; }

    //if needs targets
    public List<string> TargetingType{ get; set; }
    public List<string> EffectTargetType { get; set; }
    public List<string> EffectTargetOwner { get; set; }
    public List<string> TargetLocation { get; set; }
    public List<int> EffectTargetAmount { get; set; }
    public List<bool> AllowSelfTarget { get; set; }
    
    //if needs target stats
    public List<string> TargetStat { get; set; }
    public List<string> TargetStatCondition { get; set; }
    public List<int> TargetStatAmount { get; set; }

    //if needs trigger
    public List<string> TriggerType { get; set; }
    public List<string> TriggerCard { get; set; }
    public List<string> TriggerInfo { get; set; }
    public List<string> TriggerLocation { get; set; }
    public List<string> TriggerState { get; set; }
    public List<string> TriggerPhase { get; set; }

    //if needs scaling
    public List<string> EffectScalingType { get; set; }
    public List<string> EffectScalingCount { get; set; }
    public List<float> EfectScalingCountModifier { get; set; }
    public List<string> EffectScalingVariable { get; set; }
}