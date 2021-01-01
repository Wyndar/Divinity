using UnityEngine;
using System.Collections.Generic;
using static Effect;
using static CardLogic;
using static Game_Manager;
using static Card;
using static Player;

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

        //Open, Damaged, Death, Grave, Summon, Activation, EffectActivation, Targeting, Reinforcement, Cost, ChainResolution, Playing, Deployment, EffectResolution,TurnEnd, Undefined
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
            default:
                returnState = GameState.Undefined;
                break;
        }
        return returnState;
    }

    public Game_Manager.Phase PhaseStringToEnum(string phase)
    {
        Game_Manager.Phase returnPhase;
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

    public Effect.EffectTypes EffectTypeStringToEnum(string effectType)
    {
        Effect.EffectTypes returnEffectType;
        switch(effectType)
        {
            case "Deployment":
                returnEffectType = Effect.EffectTypes.Deployment;
                break;
            case "Chain":
                returnEffectType = Effect.EffectTypes.Chain;
                break;
            case "While Deployed":
                returnEffectType = Effect.EffectTypes.WhileDeployed;
                break;
            case "Deployed":
                returnEffectType = Effect.EffectTypes.Deployed;
                break;
            case "Vengeance":
                returnEffectType = Effect.EffectTypes.Vengeance;
                break;
            default:
                returnEffectType = Effect.EffectTypes.Undefined;
                break;
        }
        return returnEffectType;
    }

    public Effect.EffectsUsed EffectUsedStringToEnum(string effectType)
    {
        Effect.EffectsUsed returnEffectUsed;
        switch (effectType)
        {
            case "Reinforce":
                returnEffectUsed = Effect.EffectsUsed.Reinforce;
                break;
            case "Recruit":
                returnEffectUsed = Effect.EffectsUsed.Recruit;
                break;
            case "Recover":
                returnEffectUsed = Effect.EffectsUsed.Recover;
                break;
            case "Damage":
                returnEffectUsed = Effect.EffectsUsed.Damage;
                break;
            case "Regeneration":
                returnEffectUsed = Effect.EffectsUsed.Regeneration;
                break;
            case "Rally":
                returnEffectUsed = Effect.EffectsUsed.Rally;
                break;
            case "Free Revive":
                returnEffectUsed = Effect.EffectsUsed.FreeRevive;
                break;
            case "Revive":
                returnEffectUsed = Effect.EffectsUsed.Revive;
                break;
            case "Free Deploy":
                returnEffectUsed = Effect.EffectsUsed.FreeDeploy;
                break;
            case "Deploy":
                returnEffectUsed = Effect.EffectsUsed.Deploy;
                break;
            case "Vigor":
                returnEffectUsed = Effect.EffectsUsed.Vigor;
                break;
            case "Terrify":
                returnEffectUsed = Effect.EffectsUsed.Terrify;
                break;
            case "Intimidate":
                returnEffectUsed = Effect.EffectsUsed.Intimidate;
                break;
            case "Weaken":
                returnEffectUsed = Effect.EffectsUsed.Weaken;
                break;
            case "Shatter":
                returnEffectUsed = Effect.EffectsUsed.Shatter;
                break;
            case "Blood Recovery":
                returnEffectUsed = Effect.EffectsUsed.BloodRecovery;
                break;
            case "Target":
                returnEffectUsed = Effect.EffectsUsed.Target;
                break;
            case "Taunt":
                returnEffectUsed = Effect.EffectsUsed.Taunt;
                break;
            case "Stealth":
                returnEffectUsed = Effect.EffectsUsed.Stealth;
                break;
            case "Armor":
                returnEffectUsed = Effect.EffectsUsed.Armor;
                break;
            case "Sleep":
                returnEffectUsed = Effect.EffectsUsed.Sleep;
                break;
            case "Stun":
                returnEffectUsed = Effect.EffectsUsed.Stun;
                break;
            case "Provoke":
                returnEffectUsed = Effect.EffectsUsed.Provoke;
                break;
            case "Spot":
                returnEffectUsed = Effect.EffectsUsed.Spot;
                break;
            default:
                returnEffectUsed = Effect.EffectsUsed.Undefined;
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
                returnPlayType = Card.PlayType.Combatant;
                break;
            default:
                returnPlayType = Card.PlayType.Undefined;
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
}

