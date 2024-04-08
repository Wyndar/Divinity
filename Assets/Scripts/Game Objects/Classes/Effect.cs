using System.Collections.Generic;
using Unity.VisualScripting;

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
    public List<Location> ActivationLocations { get; set; }
    public int MaxActivations { get; set; }
    public int Duration { get; set; }
    //if needs trigger
    public List<EffectsUsed> TriggerEffects { get; set; }
    public List<Type> TriggerCards { get; set; }
    public Controller TriggerController { get; set; }
    public List<Location> TriggerCardLocations { get; set; }
    public List<string> TriggerInfo { get; set; }
    public List<Location> TriggerLocations { get; set; }
    public List<GameState> TriggerStates { get; set; }
    public List<Phase> TriggerPhases { get; set; }
}