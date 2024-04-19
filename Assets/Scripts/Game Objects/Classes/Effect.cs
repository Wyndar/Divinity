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

    public List<Location> ActivationLocations { get; private set; }
    public int MaxActivations { get; private set; }
    public int Duration { get; private set; }
    //if needs trigger
    public List<EffectsUsed> TriggerEffects { get; private set; }
    public List<Type> TriggerCards { get; private set; }
    public Controller TriggerController { get; private set; }
    public List<Location> TriggerCardLocations { get; private set; }
    public List<string> TriggerInfo { get; private set; }
    public List<Location> TriggerLocations { get; private set; }
    public List<GameState> TriggerStates { get; private set; }
    public List<Phase> TriggerPhases { get; private set; }
}