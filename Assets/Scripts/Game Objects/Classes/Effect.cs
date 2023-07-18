using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using static CardLogic;
using static Game_Manager;
using static Player;
using static Card;

[System.Serializable]
public class Effect
{
    public enum EffectTypes
    {
        Deployment, Chain, WhileDeployed, Deployed, Vengeance, Undefined
    }

    public enum EffectsUsed
    {
        Reinforce, Recruit, Recover, Damage, Regeneration, Rally, FreeRevive, Revive, FreeDeploy, Deploy, Vigor, Terrify, Intimidate, Weaken, Shatter, BloodRecovery, Target, Taunt, Stealth, Camouflage, Armor, Sleep, Stun, Provoke, Blind, Burn, Poison, Bomb, Spot, Bounce, Detonate, Undefined
    }

    public enum TargetingTypes
    {
        Auto, Manual, Random, Undefined
    }

    public List<EffectTypes> effectTypes;
    public List<EffectsUsed> triggerTypes;
    public List<EffectsUsed> effectsUsed;
    public Location activationLocation;
    public List<Location> triggerLocation;
    public List<Location> targetLocation;
    public List<GameState> triggerState;
    public List<Phase> triggerPhase;
    public List<Controller> effectTargetController;
    public List<TargetingTypes> targetingTypes;
    public List<Type> effectTargetType;
    public List<PlayType> effectTargetPlayType;

    public int currentActivations = new();
    public int maxActivations = new();

    public List<string> EffectType { get; set; }
    public List<string> EffectUsed { get; set; }
    public List<int> EffectAmount { get; set; }
    public List<bool> EffectActivationIsMandatory { get; set; }

    //if can be activated 
    public string ActivationLocation { get; set; }
    public int MaxActivations { get; set; }

    //if needs targets
    public List<string> TargetingType { get; set; }
    public List<string> EffectTargetType { get; set; }
    public List<string> EffectTargetPlayType { get; set; }
    public List<string> EffectTargetOwner { get; set; }
    public List<string> TargetLocation { get; set; }
    public List<int> EffectTargetAmount { get; set; }
    public List<bool> AllowSelfTarget { get; set; }

    //if should count target
    public List<float> TargetCountModifier { get; set; }

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

    //if needs scaling... needs rework
    //for now my scalers are actually stat needers
}