using System;
using System.Collections.Generic;
[Serializable]
public class EffectLogHistoryEntry : GameLogHistoryEntry
{
    public Effect loggedEffect;
    public EffectsUsed loggedEffectUsed;
    public List<CardLogic> effectTargets = new ();
    public EffectLogHistoryEntry() { }

    public EffectLogHistoryEntry(Effect loggedEffect, EffectsUsed loggedEffectUsed, List<CardLogic> loggedTargets)
    {
        this.loggedEffect = loggedEffect ?? throw new ArgumentNullException(nameof(loggedEffect));
        this.loggedEffectUsed = loggedEffectUsed;
        effectTargets.AddRange(loggedTargets);
    }
}