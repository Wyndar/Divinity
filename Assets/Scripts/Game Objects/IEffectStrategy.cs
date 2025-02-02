public interface IEffectStrategy
{
    void Execute(SubEffect subEffect, CardLogic caster, CardLogic target);
}
