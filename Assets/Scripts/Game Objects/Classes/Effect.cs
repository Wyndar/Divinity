using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class Effect
{
    public string EffectName;
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

    //empty constructor for newtonsoft to hide the internal one for our deckbuilder
    public Effect()
    {

    }
    internal Effect(Effect effect)
    {
        EffectName = effect.EffectName;
        SubEffects = new();
        foreach (SubEffect subEffect in effect.SubEffects)
        {
            SubEffect subEffectInstance = new(subEffect, this);
            SubEffects.Add(subEffectInstance);
        }
        ActivationLocations = effect.ActivationLocations;
        MaxActivations = effect.MaxActivations;
        Duration = effect.Duration;
        TriggerEffects = effect.TriggerEffects;
        TriggerCards = effect.TriggerCards;
        TriggerController = effect.TriggerController;
        TriggerCardLocations = effect.TriggerCardLocations;
        TriggerInfo = effect.TriggerInfo;
        TriggerLocations = effect.TriggerLocations;
        TriggerStates = effect.TriggerStates;
        TriggerPhases = effect.TriggerPhases;
        maxActivations = effect.MaxActivations;
        duration = effect.Duration;
        triggerCardOwner = effect.TriggerController;
        if (effect.ActivationLocations != null)
            activationLocations = new(effect.ActivationLocations);
        if (effect.TriggerLocations != null)
            triggerLocations = new(effect.TriggerLocations);
        if (effect.TriggerStates != null)
            triggerStates = new(effect.TriggerStates);
        if (effect.TriggerPhases != null)
            triggerPhases = new(effect.TriggerPhases);
        if (effect.TriggerCardLocations != null)
            triggerCardLocations = new(effect.TriggerCardLocations);
        if (effect.TriggerEffects != null)
            triggerEffects = new(effect.TriggerEffects);
    }
}