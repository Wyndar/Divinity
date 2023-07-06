using UnityEngine;

public class EnumConverter : MonoBehaviour
{
    public CardLogic.Location LocationStringToEnum(string location)
    {
        CardLogic.Location returnLocation;

        //Deck,Hand,Field,Grave,Limbo,Outside, Undefined
        switch (location)
        {
            case "Deck":
                returnLocation = CardLogic.Location.Deck;
                break;
            case "Hero Deck":
                returnLocation = CardLogic.Location.HeroDeck;
                break;
            case "Field":
                returnLocation = CardLogic.Location.Field;
                break;
            case "Grave":
                returnLocation = CardLogic.Location.Grave;
                break;
            case "Hand":
                returnLocation = CardLogic.Location.Hand;
                break;
            case "Outside":
                returnLocation = CardLogic.Location.Outside;
                break;
            default:
                returnLocation = CardLogic.Location.Undefined;
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
                returnState = Game_Manager.GameState.Open;
                break;
            case "Damaged":
                returnState = Game_Manager.GameState.Damaged;
                break;
            case "Death":
                returnState = Game_Manager.GameState.Death;
                break;
            case "Grave":
                returnState = Game_Manager.GameState.Grave;
                break;
            case "Summon":
                returnState = Game_Manager.GameState.Summon;
                break;
            case "Activation":
                returnState = Game_Manager.GameState.Activation;
                break;
            case "EffectActivation":
                returnState = Game_Manager.GameState.EffectActivation;
                break;
            case "Targeting":
                returnState = Game_Manager.GameState.Targeting;
                break;
            case "Reinforcement":
                returnState = Game_Manager.GameState.Reinforcement;
                break;
            case "Cost":
                returnState = Game_Manager.GameState.Cost;
                break;
            case "ChainResolution":
                returnState = Game_Manager.GameState.ChainResolution;
                break;
            case "Playing":
                returnState = Game_Manager.GameState.Playing;
                break;
            case "Deployment":
                returnState = Game_Manager.GameState.Deployment;
                break;
            case "Revive":
                returnState = Game_Manager.GameState.Revive;
                break;
            case "EffectResolution":
                returnState = Game_Manager.GameState.EffectResolution;
                break;
            case "TurnEnd":
                returnState = Game_Manager.GameState.TurnEnd;
                break;
            default:
                returnState = Game_Manager.GameState.Undefined;
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
                returnPhase = Game_Manager.Phase.DrawPhase;
                break;
            case "Battle Phase":
                returnPhase = Game_Manager.Phase.BattlePhase;
                break;
            case "Cost Phase":
                returnPhase =Game_Manager.Phase.CostPhase;
                break;
            case "Main Phase":
                returnPhase = Game_Manager.Phase.MainPhase;
                break;
            case "End Phase":
                returnPhase = Game_Manager.Phase.EndPhase;
                break;
            default:
                returnPhase = Game_Manager.Phase.Undefined;
                break;
        }
        return returnPhase;
    }

    public Effect.EffectType EffectTypeStringToEnum(string effectType)
    {
        Effect.EffectType returnEffectType;
        switch(effectType)
        {
            case "Deployment":
                returnEffectType = Effect.EffectType.Deployment;
                break;
            case "Chain":
                returnEffectType = Effect.EffectType.Chain;
                break;
            case "While Deployed":
                returnEffectType = Effect.EffectType.WhileDeployed;
                break;
            case "Deployed":
                returnEffectType = Effect.EffectType.Deployed;
                break;
            case "Vengeance":
                returnEffectType = Effect.EffectType.Vengeance;
                break;
            default:
                returnEffectType = Effect.EffectType.Undefined;
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
}

