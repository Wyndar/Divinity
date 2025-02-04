public enum GameState
{
    Undefined, Open, Damaged, Death, Grave, Summon, Moved, Activation, EffectActivation, Targeting, Reinforcement, Cost, ChainResolution,
    Playing, Moving, Deployment, Revive, EffectResolution, AttackDeclaration, Shielded, Bomb, Burn, Detonate, Bounce, Poison,
    Provoke, Sleep, Disarm, Stun, Silence, Buff, Debuff, Clear, TurnEnd
}

public enum Phase
{
    Undefined, DrawPhase, CostPhase, MainPhase, BattlePhase, EndPhase
}

public enum EffectTypes
{
    Undefined, Deployment, Chain, WhileDeployed, Deployed, Vengeance, Counter, Assault
}

public enum EffectsUsed
{
    Undefined, Damage, Rally, Stealth, Target, Advancement,
    Intimidate, Taunt, Armor, Camouflage, Barrier, Provoke, Disarm,
    Reinforce, Recruit, FreeDeploy, Deploy, BloodRecovery, BloodBoost,
    Weaken, Stun, Burn, Poison, Bomb, Detonate, BombDetonate, BurnDetonate, PoisonDetonate, Shatter, DeathCurse,
    FreeRevive, Revive, Vigor,  Regeneration, Immortality, Immunity, BuffDispel, DebuffDispel,
    Recover, Knockback, Pull, Shift, Bounce, Spin, Mill,


    Terrify, Sleep, Spot, Silence, ForceRetreat, Return, Retreat, Mark, Freeze,
    Discard, BloodCost
}

public enum TargetingTypes
{
    Undefined, Auto, Manual, Random, Trigger
}

public enum Type
{
    Undefined, Spell, God, Fighter, Relic, Terrain
}

public enum Attunement
{
    Undefined, Ira, Amyna, Enkrateia, Katara, Eulogia, Areta, Untuned
}

public enum PlayType
{
    Undefined, Combatant, Playable
}

public enum Trait
{
    Undefined, Food, Military, Animal, Undead, Medicine, Music, Weather, HeadOfPantheon, Commerce, Plant, Tarot, Fashion, DeadlySin,
    GreekPantheon, Mathematics
}

public enum TargetState
{
    Undefined, Default, Stealth, Taunt, Camouflage, Spot
}

public enum Debuffs
{
    Undefined, Provoked, Stunned, Sleeping, Disarmed, Burned, Poisoned, Bombed, Spotted, Silenced
}

public enum Buffs
{
    Undefined, Armor, Shield, Barrier, Stealth, Taunt, Camouflage
}

public enum Location
{
    Undefined, Deck, HeroDeck, Hand, Field, Grave, Limbo, Outside, Any
}

public enum Status
{
    Undefined, Heal, Damage, Death, HpLoss, HpGain, AtkLoss, AtkGain
}

public enum Controller
{
    Undefined, Player, Opponent, Any
}

public enum Rarity
{
    Undefined, L, UR, SR, R, UC, C
}

public enum BloodState
{
    Undefined, Active, Inactive, Locked
}

public enum DependentEffectParameter
{
    Undefined, EffectType, EffectUsed, TargetLocations, EffectTargetController, TargetingTypes, EffectTargetTypes, EffectAmount, EffectTargetAmount
}
public enum StatusType
{
    Undefined, Armor, Barrier, Burn, Poison, Stun, Taunt, Silence, Spot, Provoke, Bomb, Sleep, Disarm
}