using System.Collections.Generic;

[System.Serializable]
public class Effect
{
    public List<SubEffect> SubEffects;
    public List<Location> activationLocations;
    public List<EffectsUsed> triggerEffects;
    public List<Type> triggerCardTypes;

    public List<Location> triggerLocations;
    public List<Location> triggerCardLocations;
    public List<GameState> triggerStates;
    public List<Phase> triggerPhases;
    public Controller triggerCardOwner;

    public int currentActivations;

    public int maxActivations;
    public int duration;
    public List<string> ActivationLocations { get; set; }
    public int MaxActivations { get; set; }
    public int Duration { get; set; }
    //if needs trigger
    public List<string> TriggerEffects { get; set; }
    public List<string> TriggerCards { get; set; }
    public string TriggerOwner { get; set; }
    public List<string> TriggerCardLocations { get; set; }
    public List<string> TriggerInfo { get; set; }
    public List<string> TriggerLocations { get; set; }
    public List<string> TriggerStates { get; set; }
    public List<string> TriggerPhases { get; set; }
}