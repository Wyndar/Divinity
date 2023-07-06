using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class Effect
{

    public enum EffectType
    {
        Deployment, Chain, WhileDeployed, Deployed, Vengeance, Undefined
    }

    public enum EffectsUsed
    {
        Reinforce, Recruit, Recover, Damage, Regeneration, Rally, FreeRevive, Revive, FreeDeploy, Deploy, Vigor, Terrify, Intimidate, Weaken, Shatter, BloodRecovery, Target, Taunt, Stealth, Camouflage, Armor, Sleep, Stun, Provoke, Blind, Burn, Poison, Bomb, Spot, Undefined
    }

    public List<EffectType> effectTypes;
    public List<EffectsUsed> effectsUsed;
    public List<CardLogic.Location> activationLocation;
    public List<CardLogic.Location> triggerLocation;
    public List<Game_Manager.GameState> triggerState;
    public List<Game_Manager.Phase> triggerPhase;

    public int currentActivations;

    public List<string> EffectTypes { get; set; }
    public List<string> EffectUsed { get; set; }
    public List<int> EffectAmount { get; set; }
    public List<bool> EffectActivationIsMandatory { get; set; }

    //if can be activated 
    public string ActivationLocation { get; set; }
    public int MaxActivations { get; set; }

    //if needs targets
    public List<string> TargetingType { get; set; }
    public List<string> EffectTargetType { get; set; }
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