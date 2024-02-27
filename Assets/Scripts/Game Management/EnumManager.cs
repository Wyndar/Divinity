using UnityEngine;
using System.Collections.Generic;

public enum GameState
{
    Open, Damaged, Death, Grave, Summon, Activation, EffectActivation, Targeting, Reinforcement, Cost, ChainResolution,
    Playing, Deployment, Revive, EffectResolution, AttackDeclaration, Shielded, Bomb, Burn, Detonate, Bounce, Poison,
    Provoke, Sleep, Disarm, Stun, Silence, Buff, Debuff, Clear, TurnEnd, Undefined
}

public enum Phase
{
    DrawPhase, CostPhase, MainPhase, BattlePhase, EndPhase, Undefined
}

public enum EffectTypes
{
    Deployment, Chain, WhileDeployed, Deployed, Vengeance, Undefined
}

public enum EffectsUsed
{
    Reinforce, Recruit, Recover, Damage, Regeneration, Rally, FreeRevive, Revive, FreeDeploy, Deploy, Vigor,
    Terrify, Intimidate, Weaken, Shatter, BloodRecovery, Target, Taunt, Stealth, Camouflage, Armor, Sleep,
    Stun, Provoke, Disarm, Burn, Poison, Bomb, Spot, Bounce, Detonate, BombDetonate, BurnDetonate, PoisonDetonate,
    Barrier, BuffDispel, DebuffDispel, Silence, BloodBoost,
    Undefined
}

public enum TargetingTypes
{
    Auto, Manual, Random, Trigger, Undefined
}

public enum Type
{
    Spell, God, Fighter, Undefined
}

public enum PlayType
{
    Combatant, Playable, Undefined
}

public enum Trait
{
    Food, Military, Animal, Undead, Medicine, Music, Weather, HeadOfPantheon, Commerce, Plant, Tarot, Undefined
}

public enum TargetState
{
    Default, Stealth, Taunt, Camouflage, Spot, Undefined
}

public enum Debuffs
{
    Provoked, Stunned, Sleeping, Disarmed, Burned, Poisoned, Bombed, Spotted, Silenced, Undefined
}

public enum Buffs
{
    Armor, Shield, Barrier, Stealth, Taunt, Camouflage, Undefined
}

public enum Location
{
    Deck, HeroDeck, Hand, Field, Grave, Limbo, Outside, Any, Undefined
}

public enum Status
{
    Heal, Damage, Death, HpLoss, HpGain, AtkLoss, AtkGain, Undefined
}

public enum Controller
{
    Player, Opponent, Any, Undefined
}

public class EnumManager : MonoBehaviour
{
    public Location LocationStringToEnum(string location)
    {
        Location returnLocation = location switch
        {
            "Deck" => Location.Deck,
            "Hero Deck" => Location.HeroDeck,
            "Field" => Location.Field,
            "Grave" => Location.Grave,
            "Hand" => Location.Hand,
            "Outside" => Location.Outside,
            "Any" => Location.Any,
            _ => Location.Undefined,
        };
        return returnLocation;
    }

    public GameState StateStringToEnum(string state)
    {
        GameState returnState = state switch
        {
            "Open" => GameState.Open,
            "Damaged" => GameState.Damaged,
            "Death" => GameState.Death,
            "Grave" => GameState.Grave,
            "Summon" => GameState.Summon,
            "Activation" => GameState.Activation,
            "EffectActivation" => GameState.EffectActivation,
            "Targeting" => GameState.Targeting,
            "Reinforcement" => GameState.Reinforcement,
            "Cost" => GameState.Cost,
            "ChainResolution" => GameState.ChainResolution,
            "Playing" => GameState.Playing,
            "Deployment" => GameState.Deployment,
            "Revive" => GameState.Revive,
            "EffectResolution" => GameState.EffectResolution,
            "TurnEnd" => GameState.TurnEnd,
            "AttackDeclaration" => GameState.AttackDeclaration,
            "Shielded" => GameState.Shielded,
            "Bomb" => GameState.Bomb,
            "Burn" => GameState.Burn,
            "Detonate" => GameState.Detonate,
            "Bounce" => GameState.Bounce,
            "Poison" => GameState.Poison,
            "Stun" => GameState.Stun,
            "Provoke" => GameState.Provoke,
            "Disarm" => GameState.Disarm,
            "Sleep" => GameState.Sleep,
            "Buff" => GameState.Buff,
            "Debuff" => GameState.Debuff,
            "Clear" => GameState.Clear,
            _ => GameState.Undefined,
        };
        return returnState;
    }

    public Phase PhaseStringToEnum(string phase)
    {
        Phase returnPhase = phase switch
        {
            "Draw Phase" => Phase.DrawPhase,
            "Battle Phase" => Phase.BattlePhase,
            "Cost Phase" => Phase.CostPhase,
            "Main Phase" => Phase.MainPhase,
            "End Phase" => Phase.EndPhase,
            _ => Phase.Undefined,
        };
        return returnPhase;
    }

    public EffectTypes EffectTypeStringToEnum(string effectType)
    {
        EffectTypes returnEffectType = effectType switch
        {
            "Deployment" => EffectTypes.Deployment,
            "Chain" => EffectTypes.Chain,
            "While Deployed" => EffectTypes.WhileDeployed,
            "Deployed" => EffectTypes.Deployed,
            "Vengeance" => EffectTypes.Vengeance,
            _ => EffectTypes.Undefined,
        };
        return returnEffectType;
    }

    public EffectsUsed EffectUsedStringToEnum(string effectType)
    {
        EffectsUsed returnEffectUsed = effectType switch
        {
            "Reinforce" => EffectsUsed.Reinforce,
            "Recruit" => EffectsUsed.Recruit,
            "Recover" => EffectsUsed.Recover,
            "Damage" => EffectsUsed.Damage,
            "Regeneration" => EffectsUsed.Regeneration,
            "Rally" => EffectsUsed.Rally,
            "Free Revive" => EffectsUsed.FreeRevive,
            "Revive" => EffectsUsed.Revive,
            "Free Deploy" => EffectsUsed.FreeDeploy,
            "Deploy" => EffectsUsed.Deploy,
            "Vigor" => EffectsUsed.Vigor,
            "Terrify" => EffectsUsed.Terrify,
            "Intimidate" => EffectsUsed.Intimidate,
            "Weaken" => EffectsUsed.Weaken,
            "Shatter" => EffectsUsed.Shatter,
            "Blood Recovery" => EffectsUsed.BloodRecovery,
            "Target" => EffectsUsed.Target,
            "Taunt" => EffectsUsed.Taunt,
            "Stealth" => EffectsUsed.Stealth,
            "Camouflage" => EffectsUsed.Camouflage,
            "Armor" => EffectsUsed.Armor,
            "Sleep" => EffectsUsed.Sleep,
            "Stun" => EffectsUsed.Stun,
            "Provoke" => EffectsUsed.Provoke,
            "Disarm" => EffectsUsed.Disarm,
            "Burn" => EffectsUsed.Burn,
            "Poison" => EffectsUsed.Poison,
            "Bomb" => EffectsUsed.Bomb,
            "Spot" => EffectsUsed.Spot,
            "Bounce" => EffectsUsed.Bounce,
            "Detonate" => EffectsUsed.Detonate,
            "Bomb Detonate" => EffectsUsed.BombDetonate,
            "Burn Detonate" => EffectsUsed.BurnDetonate,
            "Poison Detonate" => EffectsUsed.PoisonDetonate,
            "Barrier" => EffectsUsed.Barrier,
            "Buff Dispel" => EffectsUsed.BuffDispel,
            "Debuff Dispel" => EffectsUsed.DebuffDispel,
            "Silence" => EffectsUsed.Silence,
            "Blood Boost" => EffectsUsed.BloodBoost,
            _ => EffectsUsed.Undefined,
        };
        return returnEffectUsed;
    }

    public List<PlayType> PlayTypeStringToEnumList(string type)
    {
        List<PlayType> returnPlayType = new();
        switch (type)
        {
            case "spell":
                returnPlayType.Add(PlayType.Playable);
                break;
            case "monster":
                returnPlayType.Add(PlayType.Playable);
                returnPlayType.Add(PlayType.Combatant);
                break;
            case "god":
                returnPlayType.Add(PlayType.Combatant);
                break;
            default:
                returnPlayType.Add(PlayType.Undefined);
                break;
        }

        return returnPlayType;
    }

    public PlayType PlayTypeStringToEnum(string type)
    {
        PlayType returnPlayType = type switch
        {
            "playable" => PlayType.Playable,
            "combatant" => PlayType.Combatant,
            _ => PlayType.Undefined,
        };
        return returnPlayType;
    }

    public Type TypeStringToEnum(string type)
    {
        Type returnType = type switch
        {
            "spell" => Type.Spell,
            "monster" => Type.Fighter,
            "god" => Type.God,
            _ => Type.Undefined,
        };
        return returnType;
    }

    public Controller OwnerStringToEnum(string player)
    {
        Controller returnController = player switch
        {
            "Player" => Controller.Player,
            "Opponent" => Controller.Opponent,
            "Any" => Controller.Any,
            _ => Controller.Undefined,
        };
        return returnController;
    }

    public TargetingTypes TargetingTypeStringToEnum(string targetingType)
    {
        TargetingTypes returnTargetingType = targetingType switch
        {
            "manual" => TargetingTypes.Manual,
            "auto" => TargetingTypes.Auto,
            "random" => TargetingTypes.Random,
            "trigger" => TargetingTypes.Trigger,
            _ => TargetingTypes.Undefined,
        };
        return returnTargetingType;
    }
    public Trait TraitStringToEnum(string trait)
    {
        // Food, Military, Animal, Undead, Medicine, Music, Weather, HeadOfPantheon, Commerce, Plant, Undefined
        Trait returnTrait = trait switch
        {
            "Food" => Trait.Food,
            "Military" => Trait.Military,
            "Weather" => Trait.Weather,
            "Animal" => Trait.Animal,
            "Undead" => Trait.Undead,
            "Medicine" => Trait.Medicine,
            "Music" => Trait.Music,
            "Head of Pantheon" => Trait.HeadOfPantheon,
            "Commerce" => Trait.Commerce,
            "Plant" => Trait.Plant,
            "Tarot"=>Trait.Tarot,
            _ => Trait.Undefined,
        };
        return returnTrait;
    }
}
