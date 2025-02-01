using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardEffectLogic: MonoBehaviour
{
    public List<Effect> effects = new();
    public SubEffect focusSubEffect;
    private CardLogic cardLogic;
    public float movementSpeed = 3f;
    private Dictionary<EffectsUsed, Action<SubEffect>> effectHandlers;
    private Dictionary<EffectsUsed, Action<SubEffect>> effectResolutions;

    public void Initialize()
    {
        cardLogic = GetComponent<CardLogic>();
        InitializeEffectHandlers();
        InitializeEffectResolutions();
    }
    private void InitializeEffectHandlers()
    {
        CombatantLogic combatantLogic = GetComponent<CombatantLogic>();
        MonsterLogic monsterLogic = GetComponent<MonsterLogic>();

        effectHandlers = new Dictionary<EffectsUsed, Action<SubEffect>>
        {
            { EffectsUsed.Rally, subEffect => combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.AtkGain) },
            { EffectsUsed.Damage, subEffect => combatantLogic.TakeDamage(subEffect.EffectAmount, false) },
            { EffectsUsed.Regeneration, subEffect => combatantLogic.Heal(subEffect.EffectAmount) },
            { EffectsUsed.Vigor, subEffect => {
                combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.AtkGain);
                combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.HpGain);
            }},
            { EffectsUsed.Terrify, subEffect => {
                combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.HpLoss);
                combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.AtkLoss);
            }},
            { EffectsUsed.Intimidate, subEffect => combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.AtkLoss) },
            { EffectsUsed.Weaken, subEffect => combatantLogic.StatAdjustment(subEffect.EffectAmount, Status.HpLoss) },
            { EffectsUsed.Shatter, subEffect => monsterLogic.MonsterDeath() },
            { EffectsUsed.Taunt, subEffect => ApplyStatusEffect<Taunt>(cardLogic, subEffect.duration, combatantLogic, TargetState.Taunt) },
            { EffectsUsed.Stealth, subEffect => ApplyStatusEffect<Stealth>(cardLogic, subEffect.duration, combatantLogic, TargetState.Stealth) },
            { EffectsUsed.Camouflage, subEffect => ApplyStatusEffect<Camouflage>(cardLogic, subEffect.duration, combatantLogic, TargetState.Stealth) },
            { EffectsUsed.Armor, subEffect => ApplyBuff<Armor>(cardLogic, subEffect.EffectAmount, subEffect.duration, combatantLogic) },
            { EffectsUsed.Barrier, subEffect => ApplyBuff<Barrier>(cardLogic, subEffect.EffectAmount, subEffect.duration, combatantLogic) },
            { EffectsUsed.Sleep, subEffect => ApplyDebuff<Sleep>(cardLogic, subEffect.duration, combatantLogic) },
            { EffectsUsed.Stun, subEffect => ApplyDebuff<Stun>(cardLogic, subEffect.duration, combatantLogic) },
            { EffectsUsed.Provoke, subEffect => ApplyStatusEffect<Provoke>(cardLogic, subEffect.duration, combatantLogic, TargetState.Stealth) },
            { EffectsUsed.Disarm, subEffect => ApplyDebuff<Disarm>(cardLogic, subEffect.duration, combatantLogic) },
            { EffectsUsed.Burn, subEffect => ApplyStatus<Burn>(cardLogic, subEffect.duration, combatantLogic, monsterLogic) },
            { EffectsUsed.Poison, subEffect => ApplyStatus<Poison>(cardLogic, subEffect.duration, combatantLogic, monsterLogic) },
            { EffectsUsed.Bomb, subEffect => ApplyStatus<Bomb>(cardLogic, subEffect.duration, combatantLogic, monsterLogic) },
            { EffectsUsed.Spot, subEffect => ApplyDebuff<Spot>(cardLogic, subEffect.duration, combatantLogic) },
            { EffectsUsed.Bounce, subEffect => StartCoroutine(monsterLogic.BounceCard()) },
            { EffectsUsed.Detonate, subEffect => DetonateStatuses(combatantLogic, cardLogic.gameManager) },
            { EffectsUsed.BombDetonate, subEffect => DetonateSpecificStatus<Bomb>(combatantLogic, cardLogic.gameManager) },
            { EffectsUsed.BurnDetonate, subEffect => DetonateSpecificStatus<Burn>(combatantLogic, cardLogic.gameManager) },
            { EffectsUsed.PoisonDetonate, subEffect => DetonateSpecificStatus<Poison>(combatantLogic, cardLogic.gameManager) },
            { EffectsUsed.BuffDispel, subEffect => DispelStatuses(combatantLogic, subEffect.EffectAmount, true) },
            { EffectsUsed.DebuffDispel, subEffect => DispelStatuses(combatantLogic, subEffect.EffectAmount, false) },
            { EffectsUsed.Silence, subEffect => ApplyDebuff<Silence>(cardLogic, subEffect.duration, combatantLogic) }
        };
    }
    private void InitializeEffectResolutions()
    {
        effectResolutions = new Dictionary<EffectsUsed, Action<SubEffect>>
        {
            { EffectsUsed.Target, subEffect => TargetEffectLogic(subEffect) },
            { EffectsUsed.BloodCost, subEffect => HandleBloodCost(subEffect) },
            { EffectsUsed.Reinforce, subEffect => StartCoroutine(cardLogic.gameManager.DrawCard(subEffect.EffectAmount, cardLogic.dataLogic.cardController)) },
            { EffectsUsed.BloodRecovery, subEffect => cardLogic.dataLogic.cardController.BloodGain(Attunement.Untuned, subEffect.EffectAmount) },
            { EffectsUsed.Recruit, subEffect => StartCoroutine(cardLogic.gameManager.SearchCard(cardLogic.targetingLogic.targets, cardLogic)) },
            { EffectsUsed.Recover, subEffect => RecoverCards() },

            // Unified handlers for similar effects
            { EffectsUsed.Damage, HandleEffectOnTargets },
            { EffectsUsed.Regeneration, HandleEffectOnTargets },
            { EffectsUsed.Shatter, HandleEffectOnTargets },
            { EffectsUsed.Detonate, HandleEffectOnTargets },
            { EffectsUsed.BurnDetonate, HandleEffectOnTargets },
            { EffectsUsed.BombDetonate, HandleEffectOnTargets },
            { EffectsUsed.PoisonDetonate, HandleEffectOnTargets },
            { EffectsUsed.Bounce, HandleEffectOnTargets },

            { EffectsUsed.Rally, HandleEffectOnTargets },
            { EffectsUsed.Vigor, HandleEffectOnTargets },
            { EffectsUsed.Taunt, HandleEffectOnTargets },
            { EffectsUsed.Stealth, HandleEffectOnTargets },
            { EffectsUsed.Armor, HandleEffectOnTargets },
            { EffectsUsed.Camouflage, HandleEffectOnTargets },
            { EffectsUsed.Barrier, HandleEffectOnTargets },

            { EffectsUsed.Terrify, HandleEffectOnTargets },
            { EffectsUsed.Intimidate, HandleEffectOnTargets },
            { EffectsUsed.Weaken, HandleEffectOnTargets },
            { EffectsUsed.Sleep, HandleEffectOnTargets },
            { EffectsUsed.Stun, HandleEffectOnTargets },
            { EffectsUsed.Provoke, HandleEffectOnTargets },
            { EffectsUsed.Disarm, HandleEffectOnTargets },
            { EffectsUsed.Burn, HandleEffectOnTargets },
            { EffectsUsed.Poison, HandleEffectOnTargets },
            { EffectsUsed.Bomb, HandleEffectOnTargets },
            { EffectsUsed.Spot, HandleEffectOnTargets },

            { EffectsUsed.FreeRevive, HandlePlayableEffects },
            { EffectsUsed.Revive, HandlePlayableEffects },
            { EffectsUsed.FreeDeploy, HandlePlayableEffects },
            { EffectsUsed.Deploy, HandlePlayableEffects }
        };
    }

    public void EffectHandler(SubEffect subEffect)
    {
        if (effectHandlers.TryGetValue(subEffect.effectUsed, out var handler))
            handler(subEffect);
        else
            throw new MissingReferenceException($"Effect '{subEffect.effectUsed}' not found");
    }

    public void EffectResolutionAfterAnimation(SubEffect subEffect)
    {
        if (effectResolutions.TryGetValue(subEffect.effectUsed, out var resolver))
            resolver(subEffect);
        else
            throw new MissingReferenceException($"Attempting to use an unimplemented effect: {subEffect.effectUsed}");
        if (!cardLogic.gameManager.isWaitingForResponse)
            FinishResolution(subEffect);
    }
    private void HandleBloodCost(SubEffect subEffect)
    {
        var attunement = Enum.Parse<Attunement>(subEffect.TargetStats[0]);
        cardLogic.dataLogic.cardController.BloodLoss(new List<Attunement> { attunement }, subEffect.EffectAmount);
    }

    private void RecoverCards()
    {
        foreach (CardLogic target in cardLogic.targetingLogic.targets)
            StartCoroutine(cardLogic.gameManager.RecoverCard(target, cardLogic));
    }

    private void HandleEffectOnTargets(SubEffect subEffect)
    {
        foreach (CardLogic target in cardLogic.targetingLogic.targets)
            target.EffectHandler(subEffect);
    }

    private void HandlePlayableEffects(SubEffect subEffect)
    {
        foreach (CardLogic target in cardLogic.targetingLogic.targets)
            target.GetComponent<PlayableLogic>().PlayCard(subEffect.effectUsed, cardLogic.dataLogic.cardController);
    }

    private void ApplyStatusEffect<T>(CardLogic cardLogic, int duration, CombatantLogic combatantLogic, TargetState targetState) where T : CardStatus
    {
        var status = (T)Activator.CreateInstance(typeof(T), cardLogic.gameManager.currentFocusCardLogic, cardLogic, duration);
        combatantLogic.SetTargetStatus(status, targetState);
    }

    private void ApplyBuff<CardStatus>(CardLogic cardLogic, int amount, int duration, CombatantLogic combatantLogic)
    {
        var buff = (Buff)Activator.CreateInstance(typeof(Buff), cardLogic.gameManager.currentFocusCardLogic, cardLogic, amount, duration);
        combatantLogic.AddNonStackingBuff(buff);
    }

    private void ApplyDebuff<CardStatus>(CardLogic cardLogic, int duration, CombatantLogic combatantLogic)
    {
        var debuff = (Debuff)Activator.CreateInstance(typeof(Debuff), cardLogic.gameManager.currentFocusCardLogic, cardLogic, duration);
        combatantLogic.AddNonStackingDebuff(debuff);
    }

    private void ApplyStatus<T>(CardLogic cardLogic, int duration, CombatantLogic combatantLogic, MonsterLogic monsterLogic) where T : CardStatus
    {
        var status = (T)Activator.CreateInstance(typeof(T), cardLogic.gameManager.currentFocusCardLogic, cardLogic, duration);
        combatantLogic.cardStatuses.Add(status);
        monsterLogic.currentSlot.SetStatusIcon(status);
    }

    private void DetonateStatuses(CombatantLogic combatantLogic, GameBattleManager gameManager)
    {
        foreach (var status in combatantLogic.cardStatuses.ToList())
            status.DetonateActions(gameManager);
    }

    private void DetonateSpecificStatus<T>(CombatantLogic combatantLogic, GameBattleManager gameManager) where T : CardStatus
    {
        foreach (var status in combatantLogic.cardStatuses.OfType<T>())
            status.DetonateActions(gameManager);
    }

    private void DispelStatuses(CombatantLogic combatantLogic, int amount, bool isBuff)
    {
        for (int i = 0; i < amount; i++)
            if (combatantLogic.cardStatuses.Count > 0)
            {
                CardStatus status = isBuff ? combatantLogic.BuffCheck(Buffs.Undefined) : combatantLogic.DebuffCheck(Debuffs.Undefined);
                if (status != null)
                    combatantLogic.RemoveCardStatus(status);
            }
    }

public void EffectActivation(SubEffect subEffect)
    {
        if (subEffect.effectUsed == EffectsUsed.BloodCost &&
            cardLogic.dataLogic.cardController.BloodAttunementCheck(Enum.Parse<Attunement>(subEffect.TargetStats[0])) < subEffect.effectAmount)
            return;
        cardLogic.gameManager.isActivatingEffect = true;
        cardLogic.gameManager.DisableRayBlocker();
        if (subEffect.effectType != EffectTypes.WhileDeployed)
        {
            cardLogic.gameManager.StateChange(GameState.EffectActivation);
            cardLogic.audioManager.NewAudioPrefab(cardLogic.audioManager.effectActivation);
            if (cardLogic.dataLogic.type != Type.God)
            {
                StartCoroutine(TweenAnimationCoroutine(subEffect, true));
                return;
            }
        }
        EffectActivationAfterAnimation(subEffect);
    }

    private IEnumerator TweenAnimationCoroutine(SubEffect subEffect, bool isActivation)
    {
        float distance = Vector3.Distance(transform.position, cardLogic.dataLogic.cardController.activationZone.position);
        Vector3 originalPosition = transform.position;
        Vector3 direction = (cardLogic.dataLogic.cardController.activationZone.position - transform.position).normalized;
        float distanceTravelled = 0;
        while (distanceTravelled < distance)
        {
            Vector3 translationDistance = (cardLogic.dataLogic.cardController.activationZone.position - transform.position);
            if (Vector3.SqrMagnitude(translationDistance) <= Vector3.SqrMagnitude(direction))
                transform.position = cardLogic.dataLogic.cardController.activationZone.position;
            else
                transform.Translate(direction * movementSpeed, Space.World);
            distanceTravelled = Vector3.Distance(originalPosition, transform.position);
            yield return null;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = new(originalScale.x * 2.5f, originalScale.y * 2.5f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        if (isActivation)
            EffectActivationAfterAnimation(subEffect);
        else
            EffectResolutionAfterAnimation(subEffect);
        yield return null;
    }

    private void EffectActivationAfterAnimation(SubEffect subEffect)
    {
        focusSubEffect = subEffect;
        if (subEffect.parentEffect.activationLocations == null 
            || subEffect.parentEffect.activationLocations.Contains(cardLogic.dataLogic.currentLocation))
            cardLogic.targetingLogic.TargetCheck(subEffect);
    }

    public void EffectResolution(SubEffect subEffect)
    {
        cardLogic.gameManager.DisableRayBlocker();
        if (subEffect.effectType != EffectTypes.WhileDeployed)
        {
            cardLogic.gameManager.StateChange(GameState.EffectResolution);
            if (cardLogic.dataLogic.type != Type.God)
            {
                StartCoroutine(TweenAnimationCoroutine(subEffect, false));
                return;
            }
        }
        EffectResolutionAfterAnimation(subEffect);
    }

    private void TargetEffectLogic(SubEffect subEffect)
    {
        int subEffectIndex = subEffect.parentEffect.SubEffects.FindIndex(a => a == subEffect);
        //use big E cos small e is supposed to be editable and change
        foreach (SubEffect sub in subEffect.parentEffect.SubEffects)
        {
            int subIndex = subEffect.parentEffect.SubEffects.FindIndex(a => a == sub);
            if (sub.DependentEffectParameters == null || subIndex <= subEffectIndex)
                continue;
            float mod = sub.TargetCountModifier > 0 ? sub.TargetCountModifier : 1;
            foreach (DependentEffectParameter dependent in sub.DependentEffectParameters)
            {
                int targetVariableIndex = sub.DependentEffectParameters.FindIndex(a => a == dependent);
                if (targetVariableIndex != -1 && sub.DependentIndices[targetVariableIndex] == subEffectIndex)
                    switch (dependent)
                    {
                        case DependentEffectParameter.EffectTargetAmount:
                            sub.effectTargetAmount = subEffect.TargetStats == null
                            ? Mathf.FloorToInt(cardLogic.targetingLogic.targets.Count * mod)
                            : GetModifiedDepenndentParameterValue(subEffect.TargetStats[targetVariableIndex], mod);
                            break;
                        case DependentEffectParameter.EffectAmount:
                            sub.effectAmount = subEffect.TargetStats == null
                            ? Mathf.FloorToInt(cardLogic.targetingLogic.targets.Count * mod)
                            : GetModifiedDepenndentParameterValue(subEffect.TargetStats[targetVariableIndex], mod);
                            break;
                        default:
                            throw new MissingReferenceException("Unimplemented dependent parameter required for effect");
                    }
            }
        }
    }
    private int GetModifiedDepenndentParameterValue(string checkedStat, float mod)
    {
        cardLogic.targetingLogic.targets[0].TryGetComponent<CombatantLogic>(out var combatant);
        cardLogic.targetingLogic.targets[0].TryGetComponent<PlayableLogic>(out var playable);
        return checkedStat switch
        {
            "current atk" => Mathf.CeilToInt(combatant.currentAtk * mod),
            "cost" => Mathf.CeilToInt(playable.cost * mod),
            _ => throw new MissingReferenceException("unimplemented target stat"),
        };
    }

    public void FinishResolution(SubEffect subEffect)
    {
        cardLogic.gameManager.InvokeEffectTrigger(subEffect, cardLogic);
        CheckSubsequentEffects(subEffect, true);
    }

    private void CheckSubsequentEffects(SubEffect subEffect, bool resolvedPreviousSubEffect)
    {
        if (!ResolveSubsequentSubeffects(subEffect))
            return;
        if (resolvedPreviousSubEffect && subEffect.parentEffect.currentActivations < subEffect.parentEffect.maxActivations)
            subEffect.parentEffect.currentActivations++;
        cardLogic.gameManager.ClearEffectTargetImages();
        cardLogic.targetingLogic.targets?.Clear();
        cardLogic.targetingLogic.validTargets?.Clear();
        if (cardLogic.dataLogic.type == Type.Spell)
            GetComponent<PlayableLogic>().MoveToGrave();
        cardLogic.audioManager.NewAudioPrefab(cardLogic.audioManager.effectResolution);
        if (cardLogic.dataLogic.cardController.isAI)
            cardLogic.dataLogic.cardController.AIManager.isPerformingAction = false;
        cardLogic.gameManager.isActivatingEffect = false;
        cardLogic.gameManager.ChainResolution();
    }

    private bool ResolveSubsequentSubeffects(SubEffect subEffect)
    {
        int subCount = subEffect.parentEffect.SubEffects.FindIndex(a => a == subEffect);
        if (subCount+1 >= subEffect.parentEffect.SubEffects.Count)
            return true;
        SubEffect nextSubEffect = subEffect.parentEffect.SubEffects[subCount+1];
        if (subEffect.effectType != nextSubEffect.effectType)
            return true;
        if (nextSubEffect.EffectActivationIsMandatory == false)
        {
            focusSubEffect = nextSubEffect;
            cardLogic.SetFocusCardLogic();
            if (cardLogic.dataLogic.cardController.isAI)
                OptionalEffectResolution(cardLogic.dataLogic.cardController.AIManager.ActivateOptionalEffect());
            else
                cardLogic.gameManager.EnableActivationPanel();
            return false;
        }
        if (nextSubEffect.EffectTargetAmount == 98)
            EffectResolution(nextSubEffect);
        else
        {
            cardLogic.targetingLogic.targets?.Clear();
            cardLogic.targetingLogic.validTargets?.Clear();
            cardLogic.targetingLogic.TargetCheck(nextSubEffect);
        }
        return false;
    }
    public void OptionalEffectResolution(bool used)
    {
        if (!used)
        {
            CheckSubsequentEffects(focusSubEffect, used);
            return;
        }
        //if you need the targets from previous effect to resolve
        if (focusSubEffect.effectTargetAmount == 98)
            EffectResolution(focusSubEffect);
        else
        {
            cardLogic.targetingLogic.targets.Clear();
            cardLogic.targetingLogic.validTargets.Clear();
            EffectActivation(focusSubEffect);
        }
        //otherwise activate effect afresh
    }
    public bool IsValidEffect()
    {
        if (effects != null)
            foreach (var _ in effects.Where(effect => effect.currentActivations < effect.maxActivations
            && effect.SubEffects[0].effectType == EffectTypes.Deployed)
                .Where(effect => effect.SubEffects[0].effectUsed != EffectsUsed.BloodCost
                || cardLogic.dataLogic.cardController.BloodAttunementCheck(Enum.Parse<Attunement>(effect.SubEffects[0].TargetStats[0]))
                >= effect.SubEffects[0].effectAmount).Select(effect => new { }))
                return true;
        return false;
    }
    //reset activation count for effects usually at turn start
    public void EffectRefresh()
    {
        foreach (Effect effect in effects)
            effect.currentActivations = 0;
    }
    public void EffectLogger(SubEffect subEffect, List<CardLogic> cards)
    {
        Effect effect = subEffect.parentEffect;
        EffectLogHistoryEntry effectLogHistoryEntry = new(effect, subEffect.effectUsed, cards)
        {
            logIndex = cardLogic.gameManager.gameLogHistoryEntries.Count,
            loggedCard = cardLogic,
            loggedLocation = cardLogic.dataLogic.currentLocation
        };
        cardLogic.gameManager.gameLogHistoryEntries.Add(effectLogHistoryEntry);
    }
}
