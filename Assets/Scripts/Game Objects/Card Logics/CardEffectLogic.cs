using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardEffectLogic : MonoBehaviour
{
    public List<Effect> effects = new();
    public SubEffect focusSubEffect;
    private CardLogic cardLogic;
    public float movementSpeed = 3f;

    private Dictionary<EffectsUsed, Lazy<IEffectStrategy>> effectHandlers;
    private Dictionary<EffectsUsed, Lazy<IEffectStrategy>> effectResolutions;

    public void Initialize()
    {
        cardLogic = GetComponent<CardLogic>();
        InitializeEffectHandlers();
        InitializeEffectResolutions();
    }

    private void InitializeEffectHandlers()
    {
        effectHandlers = new Dictionary<EffectsUsed, Lazy<IEffectStrategy>>
        {
            { EffectsUsed.Rally, new Lazy<IEffectStrategy>(()=>new RallyEffect()) },
            { EffectsUsed.Damage, new Lazy < IEffectStrategy > (() => new DamageEffect()) },
            { EffectsUsed.Regeneration, new Lazy < IEffectStrategy > (() => new RegenerationEffect()) },
            { EffectsUsed.Vigor, new Lazy < IEffectStrategy > (() => new VigorEffect()) },
            { EffectsUsed.Terrify, new Lazy < IEffectStrategy > (() => new TerrifyEffect()) },
            { EffectsUsed.Intimidate, new Lazy < IEffectStrategy > (() => new IntimidateEffect()) },
            { EffectsUsed.Weaken, new Lazy < IEffectStrategy > (() => new WeakenEffect()) },
            { EffectsUsed.Shatter, new Lazy < IEffectStrategy > (() => new ShatterEffect()) },
            { EffectsUsed.Taunt, new Lazy < IEffectStrategy > (() => new TauntEffect()) },
            { EffectsUsed.Stealth, new Lazy < IEffectStrategy > (() => new StealthEffect()) },
            { EffectsUsed.Camouflage, new Lazy < IEffectStrategy > (() => new CamouflageEffect()) },
            { EffectsUsed.Armor, new Lazy < IEffectStrategy > (() => new ArmorEffect()) },
            { EffectsUsed.Barrier, new Lazy < IEffectStrategy > (() => new BarrierEffect()) },
            { EffectsUsed.Sleep, new Lazy < IEffectStrategy > (() => new SleepEffect()) },
            { EffectsUsed.Stun, new Lazy < IEffectStrategy > (() => new StunEffect()) },
            { EffectsUsed.Provoke, new Lazy < IEffectStrategy > (() => new ProvokeEffect()) },
            { EffectsUsed.Disarm, new Lazy < IEffectStrategy > (() => new DisarmEffect()) },
            { EffectsUsed.Burn, new Lazy < IEffectStrategy > (() => new BurnEffect()) },
            { EffectsUsed.Poison, new Lazy < IEffectStrategy > (() => new PoisonEffect()) },
            { EffectsUsed.Bomb, new Lazy < IEffectStrategy > (() => new BombEffect()) },
            { EffectsUsed.Spot, new Lazy < IEffectStrategy > (() => new SpotEffect()) },
            { EffectsUsed.Bounce, new Lazy < IEffectStrategy > (() => new BounceEffect()) },
            { EffectsUsed.Detonate, new Lazy < IEffectStrategy > (() => new DetonateEffect()) },
            { EffectsUsed.BombDetonate, new Lazy < IEffectStrategy > (() => new BombDetonateEffect()) },
            { EffectsUsed.BurnDetonate, new Lazy < IEffectStrategy > (() => new BurnDetonateEffect()) },
            { EffectsUsed.PoisonDetonate, new Lazy < IEffectStrategy > (() => new PoisonDetonateEffect()) },
            { EffectsUsed.BuffDispel, new Lazy < IEffectStrategy > (() => new BuffDispelEffect()) },
            { EffectsUsed.DebuffDispel, new Lazy < IEffectStrategy > (() => new DebuffDispelEffect()) },
            { EffectsUsed.Silence, new Lazy < IEffectStrategy > (() => new SilenceEffect()) }
        };
    }

    private void InitializeEffectResolutions()
    {
        effectResolutions = new Dictionary<EffectsUsed, Lazy<IEffectStrategy>>
        {
            { EffectsUsed.Target, new Lazy < IEffectStrategy > (() => new TargetEffect()) },
            { EffectsUsed.BloodCost, new Lazy < IEffectStrategy > (() => new BloodCostEffect()) },
            { EffectsUsed.Reinforce, new Lazy < IEffectStrategy > (() => new ReinforceEffect()) },
            { EffectsUsed.BloodRecovery, new Lazy < IEffectStrategy > (() => new BloodRecoveryEffect()) },
            { EffectsUsed.Recruit, new Lazy < IEffectStrategy > (() => new RecruitEffect()) },
            { EffectsUsed.Recover, new Lazy < IEffectStrategy > (() => new RecoverEffect()) },

            { EffectsUsed.Damage, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Regeneration, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Shatter, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Detonate, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.BurnDetonate, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.BombDetonate, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.PoisonDetonate, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Bounce, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },

            { EffectsUsed.Rally, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Vigor, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Taunt, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Stealth, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Armor, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Camouflage, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Barrier, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },

            { EffectsUsed.Terrify, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Intimidate, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Weaken, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Sleep, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Stun, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Provoke, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Disarm, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Burn, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Poison, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Bomb, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },
            { EffectsUsed.Spot, new Lazy < IEffectStrategy > (() => new GeneralEffect()) },

            { EffectsUsed.FreeRevive, new Lazy < IEffectStrategy > (() => new PlayableEffect()) },
            { EffectsUsed.Revive, new Lazy < IEffectStrategy > (() => new PlayableEffect()) },
            { EffectsUsed.FreeDeploy, new Lazy < IEffectStrategy > (() => new PlayableEffect()) },
            { EffectsUsed.Deploy, new Lazy < IEffectStrategy > (() => new PlayableEffect()) }
        };
    }
    public void EffectHandler(SubEffect subEffect)
    {
        if (effectHandlers.TryGetValue(subEffect.effectUsed, out Lazy<IEffectStrategy> strategy))
                strategy.Value.Execute(subEffect, cardLogic, cardLogic);
        else throw new MissingReferenceException($"Effect '{subEffect.effectUsed}' not found.");
    }

    public void EffectResolutionAfterAnimation(SubEffect subEffect)
    {
        if (effectResolutions.TryGetValue(subEffect.effectUsed, out Lazy<IEffectStrategy> strategy))
        {
            if (cardLogic.targetingLogic.targets.Count == 0)
            {
                if (subEffect.effectTargetAmount > 0)
                    FinishResolution(subEffect);
                else
                    strategy.Value.Execute(subEffect, cardLogic, cardLogic);
            }
            else foreach (var target in cardLogic.targetingLogic.targets)
                    strategy.Value.Execute(subEffect, cardLogic, target);
        }
        else throw new MissingReferenceException($"Effect '{subEffect.effectUsed}' not found.");
        if (!cardLogic.gameManager.isWaitingForResponse)
            FinishResolution(subEffect);
    }
    public void EffectActivation(SubEffect subEffect)
    {
        if (subEffect.effectUsed == EffectsUsed.BloodCost &&
            cardLogic.dataLogic.cardController.BloodAttunementCheck(Enum.Parse<Attunement>(subEffect.TargetStats[0]))
            < subEffect.effectAmount)
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
        if (subEffect.ParentEffect.activationLocations == null 
            || subEffect.ParentEffect.activationLocations.Contains(cardLogic.dataLogic.currentLocation))
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

    public void FinishResolution(SubEffect subEffect)
    {
        EffectLogger(subEffect, cardLogic.targetingLogic.targets);
        cardLogic.gameManager.InvokeEffectTrigger(subEffect, cardLogic);
        CheckSubsequentEffects(subEffect, true);
    }

    private void CheckSubsequentEffects(SubEffect subEffect, bool resolvedPreviousSubEffect)
    {
        if (!ResolveSubsequentSubeffects(subEffect))
            return;
        if (resolvedPreviousSubEffect && subEffect.ParentEffect.currentActivations < subEffect.ParentEffect.maxActivations)
            subEffect.ParentEffect.currentActivations++;
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
        int subCount = subEffect.ParentEffect.SubEffects.FindIndex(a => a == subEffect);
        if (subCount+1 >= subEffect.ParentEffect.SubEffects.Count)
            return true;
        SubEffect nextSubEffect = subEffect.ParentEffect.SubEffects[subCount+1];
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
    private void EffectLogger(SubEffect subEffect, List<CardLogic> cards)
    {
        Effect effect = subEffect.ParentEffect;
        EffectLogHistoryEntry effectLogHistoryEntry = new(effect, subEffect.effectUsed, cards)
        {
            logIndex = cardLogic.gameManager.gameLogHistoryEntries.Count,
            loggedCard = cardLogic,
            loggedLocation = cardLogic.dataLogic.currentLocation
        };
        cardLogic.gameManager.gameLogHistoryEntries.Add(effectLogHistoryEntry);
    }
}
