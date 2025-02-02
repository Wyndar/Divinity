using System;

internal class PlayableEffect : IEffectStrategy
{
    private static readonly Lazy<PlayableEffect> _instance = new(() => new PlayableEffect());
    public static PlayableEffect Instance => _instance.Value;
    public void Execute(SubEffect subEffect, CardLogic caster, CardLogic target)
    {
        target.GetComponent<PlayableLogic>().PlayCard(subEffect.effectUsed, caster.dataLogic.cardController);
    }
}