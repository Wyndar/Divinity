using UnityEngine;
using System.Collections.Generic;
using static Effect;
using static CardLogic;
using static Game_Manager;
using static Card;
using static Player;
using Unity.VisualScripting;

public class EnumConverter : MonoBehaviour
{
    public Location LocationStringToEnum(string location)
    {
        Location returnLocation;

        //Deck,Hand,Field,Grave,Limbo,Outside, Undefined
        switch (location)
        {
            case "Deck":
                returnLocation = Location.Deck;
                break;
            case "Hero Deck":
                returnLocation = Location.HeroDeck;
                break;
            case "Field":
                returnLocation = Location.Field;
                break;
            case "Grave":
                returnLocation = Location.Grave;
                break;
            case "Hand":
                returnLocation = Location.Hand;
                break;
            case "Outside":
                returnLocation = Location.Outside;
                break;
            case "Any":
                returnLocation = Location.Any;
                break;
            default:
                returnLocation = Location.Undefined;
                break;
        }
        return returnLocation;
    }

    public Game_Manager.GameState StateStringToEnum(string state)
    {
        Game_Manager.GameState returnState;

        //Open, Damaged, Death, Grave, Summon, Activation, EffectActivation, Targeting, Reinforcement, Cost, ChainResolution, Playing, Deployment, EffectResolution, TurnEnd, AttackDeclaration, Shielded, Bomb, Burn, Detonate, Bounce, Poison, Provoke, Sleep, Blind, Stun,Buff,Debuff, Clear, Undefined
        switch (state)
        {
            case "Open":
                returnState = GameState.Open;
                break;
            case "Damaged":
                returnState = GameState.Damaged;
                break;
            case "Death":
                returnState = GameState.Death;
                break;
            case "Grave":
                returnState = GameState.Grave;
                break;
            case "Summon":
                returnState = GameState.Summon;
                break;
            case "Activation":
                returnState = GameState.Activation;
                break;
            case "EffectActivation":
                returnState = GameState.EffectActivation;
                break;
            case "Targeting":
                returnState = GameState.Targeting;
                break;
            case "Reinforcement":
                returnState = GameState.Reinforcement;
                break;
            case "Cost":
                returnState = GameState.Cost;
                break;
            case "ChainResolution":
                returnState = GameState.ChainResolution;
                break;
            case "Playing":
                returnState = GameState.Playing;
                break;
            case "Deployment":
                returnState = GameState.Deployment;
                break;
            case "Revive":
                returnState = GameState.Revive;
                break;
            case "EffectResolution":
                returnState = GameState.EffectResolution;
                break;
            case "TurnEnd":
                returnState = GameState.TurnEnd;
                break;
            case "AttackDeclaration":
                returnState = GameState.AttackDeclaration;
                break;
            case "Shielded":
                returnState = GameState.Shielded;
                break;
            case "Bomb":
                returnState = GameState.Bomb;
                break;
            case "Burn":
                returnState = GameState.Burn;
                break;
            case "Detonate":
                returnState = GameState.Detonate;
                break;
            case "Bounce":
                returnState = GameState.Bounce;
                break;
            case "Poison":
                returnState = GameState.Poison;
                break;
            case "Stun":
                returnState = GameState.Stun;
                break;
            case "Provoke":
                returnState = GameState.Provoke;
                break;
            case "Blind":
                returnState = GameState.Blind;
                break;
            case "Sleep":
                returnState = GameState.Sleep;
                break;
            case "Buff":
                returnState = GameState.Buff;
                break;
            case "Debuff":
                returnState = GameState.Debuff;
                break;
            case "Clear":
                returnState = GameState.Clear;
                break;
            default:
                returnState = GameState.Undefined;
                break;
        }
        return returnState;
    }

    public Phase PhaseStringToEnum(string phase)
    {
        Phase returnPhase;
        //DrawPhase,CostPhase,MainPhase,BattlePhase,EndPhase,Undefined
        switch (phase)
        {
            case "Draw Phase":
                returnPhase = Phase.DrawPhase;
                break;
            case "Battle Phase":
                returnPhase = Phase.BattlePhase;
                break;
            case "Cost Phase":
                returnPhase =Phase.CostPhase;
                break;
            case "Main Phase":
                returnPhase = Phase.MainPhase;
                break;
            case "End Phase":
                returnPhase = Phase.EndPhase;
                break;
            default:
                returnPhase = Phase.Undefined;
                break;
        }
        return returnPhase;
    }

    public EffectTypes EffectTypeStringToEnum(string effectType)
    {
        EffectTypes returnEffectType;
        switch(effectType)
        {
            case "Deployment":
                returnEffectType = EffectTypes.Deployment;
                break;
            case "Chain":
                returnEffectType = EffectTypes.Chain;
                break;
            case "While Deployed":
                returnEffectType = EffectTypes.WhileDeployed;
                break;
            case "Deployed":
                returnEffectType = EffectTypes.Deployed;
                break;
            case "Vengeance":
                returnEffectType = EffectTypes.Vengeance;
                break;
            default:
                returnEffectType = EffectTypes.Undefined;
                break;
        }
        return returnEffectType;
    }

    public Effect.EffectsUsed EffectUsedStringToEnum(string effectType)
    {
        Effect.EffectsUsed returnEffectUsed;
        //  Reinforce, Recruit, Recover, Damage, Regeneration, Rally, FreeRevive, Revive, FreeDeploy, Deploy, Vigor, Terrify, Intimidate, Weaken, Shatter, BloodRecovery, Target, Taunt, Stealth, Camouflage, Armor, Sleep, Stun, Provoke, Blind, Burn, Poison, Bomb, Spot, Bounce, Detonate, Undefined
        switch (effectType)
        {
            case "Reinforce":
                returnEffectUsed = EffectsUsed.Reinforce;
                break;
            case "Recruit":
                returnEffectUsed = EffectsUsed.Recruit;
                break;
            case "Recover":
                returnEffectUsed = EffectsUsed.Recover;
                break;
            case "Damage":
                returnEffectUsed = EffectsUsed.Damage;
                break;
            case "Regeneration":
                returnEffectUsed = EffectsUsed.Regeneration;
                break;
            case "Rally":
                returnEffectUsed = EffectsUsed.Rally;
                break;
            case "Free Revive":
                returnEffectUsed = EffectsUsed.FreeRevive;
                break;
            case "Revive":
                returnEffectUsed = EffectsUsed.Revive;
                break;
            case "Free Deploy":
                returnEffectUsed = EffectsUsed.FreeDeploy;
                break;
            case "Deploy":
                returnEffectUsed = EffectsUsed.Deploy;
                break;
            case "Vigor":
                returnEffectUsed = EffectsUsed.Vigor;
                break;
            case "Terrify":
                returnEffectUsed = EffectsUsed.Terrify;
                break;
            case "Intimidate":
                returnEffectUsed = EffectsUsed.Intimidate;
                break;
            case "Weaken":
                returnEffectUsed = EffectsUsed.Weaken;
                break;
            case "Shatter":
                returnEffectUsed = EffectsUsed.Shatter;
                break;
            case "Blood Recovery":
                returnEffectUsed = EffectsUsed.BloodRecovery;
                break;
            case "Target":
                returnEffectUsed = EffectsUsed.Target;
                break;
            case "Taunt":
                returnEffectUsed = EffectsUsed.Taunt;
                break;
            case "Stealth":
                returnEffectUsed = EffectsUsed.Stealth;
                break;
            case "Camouflage":
                returnEffectUsed = EffectsUsed.Camouflage;
                break;
            case "Armor":
                returnEffectUsed = EffectsUsed.Armor;
                break;
            case "Sleep":
                returnEffectUsed = EffectsUsed.Sleep;
                break;
            case "Stun":
                returnEffectUsed = EffectsUsed.Stun;
                break;
            case "Provoke":
                returnEffectUsed = EffectsUsed.Provoke;
                break;
            case "Blind":
                returnEffectUsed = EffectsUsed.Blind;
                break;
            case "Burn":
                returnEffectUsed = EffectsUsed.Burn;
                break;
            case "Poison":
                returnEffectUsed = EffectsUsed.Poison;
                break;
            case "Bomb":
                returnEffectUsed = EffectsUsed.Bomb;
                break;
            case "Spot":
                returnEffectUsed = EffectsUsed.Spot;
                break;
            case "Bounce":
                returnEffectUsed = EffectsUsed.Bounce;
                break;
            case "Detonate":
                returnEffectUsed = EffectsUsed.Detonate;
                break;
            default:
                returnEffectUsed = EffectsUsed.Undefined;
                break;
        }
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
        PlayType returnPlayType;
        switch (type)
        {
            case "playable":
                returnPlayType = PlayType.Playable;
                break;
            case "combatant":
                returnPlayType = PlayType.Combatant;
                break;
            default:
                returnPlayType = PlayType.Undefined;
                break;
        }
        return returnPlayType;
    }

    public Type TypeStringToEnum(string type)
    {
        Type returnType;
        switch (type)
        {
            case "spell":
                returnType = Type.Spell;
                break;
            case "monster":
                returnType = Type.Fighter;
                break;
            case "god":
                returnType = Type.God;
                break;
            default:
                returnType = Type.Undefined;
                break;
        }
        return returnType;
    }

    public Controller OwnerStringToEnum(string player)
    {
        Controller returnController;
        switch(player)
        {
            case "Player":
                returnController = Controller.Player;
                break;
            case "Opponent":
                returnController = Controller.Opponent;
                break;
            case "Any":
                returnController = Controller.Any;
                break;
            default:
                returnController = Controller.Undefined;
                break;
        }
        return returnController;
    }

    public TargetingTypes TargetingTypeStringToEnum(string targetingType)
    {
        TargetingTypes returnTargetingType;
        switch(targetingType)
        {
            case "manual":
                returnTargetingType = TargetingTypes.Manual;
                break;
            case "auto":
                returnTargetingType = TargetingTypes.Auto;
                break;
            case "random":
                returnTargetingType = TargetingTypes.Random;
                break;
            default:
                returnTargetingType = TargetingTypes.Undefined;
                break;
        }
        return returnTargetingType;
    }
    public Trait TraitStringToEnum(string trait)
    {
        // Food, Military, Animal, Undead, Medicine, Music, Weather, HeadOfPantheon, Commerce, Plant, Undefined
        Trait returnTrait;
        switch(trait)
        {
            case"Food":
                returnTrait = Trait.Food;
                break;
            case "Military":
                returnTrait = Trait.Military;
                break;
            case "Weather":
                returnTrait = Trait.Weather;
                break;
            case "Animal":
                returnTrait = Trait.Animal;
                break;
            case "Undead":
                returnTrait = Trait.Undead;
                break;
            case "Medicine":
                returnTrait = Trait.Medicine;
                break;
            case "Music":
                returnTrait = Trait.Music;
                break;
            case "Head of Pantheon":
                returnTrait = Trait.HeadOfPantheon;
                break;
            case "Commerce":
                returnTrait = Trait.Commerce;
                break;
            case "Plant":
                returnTrait = Trait.Plant;
                break;
            default:
                returnTrait = Trait.Undefined;
                break;
        }
        return returnTrait;
    }
}

