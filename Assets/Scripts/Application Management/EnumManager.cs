public enum GameState
{
    Open, Damaged, Death, Grave, Summon, Moved, Activation, EffectActivation, Targeting, Reinforcement, Cost, ChainResolution,
    Playing, Moving, Deployment, Revive, EffectResolution, AttackDeclaration, Shielded, Bomb, Burn, Detonate, Bounce, Poison,
    Provoke, Sleep, Disarm, Stun, Silence, Buff, Debuff, Clear, TurnEnd, Undefined
}

public enum Phase
{
    DrawPhase, CostPhase, MainPhase, BattlePhase, EndPhase, Undefined
}

public enum EffectTypes
{
    Deployment, Chain, WhileDeployed, Deployed, Vengeance, Counter, Assault, Undefined
}

public enum EffectsUsed
{
    Damage, Rally, Stealth, Target, Advancement,
    Intimidate, Taunt, Armor, Camouflage, Barrier, Provoke, Disarm,
    Reinforce, Recruit, FreeDeploy, Deploy, BloodRecovery, BloodBoost,
    Weaken, Stun, Burn, Poison, Bomb, Detonate, BombDetonate, BurnDetonate, PoisonDetonate, Shatter, DeathCurse,
    FreeRevive, Revive, Vigor,  Regeneration, Immortality, Immunity, BuffDispel, DebuffDispel,
    Recover, Knockback, Pull, Shift, Bounce, Spin, Mill,


    Terrify, Sleep, Spot, Silence, ForceRetreat, Return, Retreat, Mark, Freeze,
    Discard, BloodCost, Undefined
}

public enum TargetingTypes
{
    Auto, Manual, Random, Trigger, Undefined
}

public enum Type
{
    Spell, God, Fighter, Relic, Terrain, Undefined
}

public enum Attunement
{
    Ira, Amyna, Enkrateia, Katara, Eulogia, Areta, Untuned, Undefined
}

public enum PlayType
{
    Combatant, Playable, Undefined
}

public enum Trait
{
    Food, Military, Animal, Undead, Medicine, Music, Weather, HeadOfPantheon, Commerce, Plant, Tarot, Fashion, DeadlySin,
    GreekPantheon, Mathematics, Undefined
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

public enum Rarity
{
    L, UR, SR, R, UC, C, Undefined
}

public enum BloodState
{
    Active, Inactive, Locked, Undefined
}

public enum DependentEffectParameter
{
    EffectType, EffectUsed, TargetLocations, EffectTargetController, TargetingTypes, EffectTargetTypes, EffectAmount, EffectTargetAmount
}