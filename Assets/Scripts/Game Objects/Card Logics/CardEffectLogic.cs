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
    public void Initialize() => cardLogic = GetComponent<CardLogic>();
    public void EffectHandler(SubEffect subEffect)
    {
        //apart from where specified, duration set to 0(not defined) indicates an infinite duration, be careful with blanks
        CombatantLogic combatantLogic = GetComponent<CombatantLogic>();
        MonsterLogic monsterLogic = GetComponent<MonsterLogic>();
        CardLogic logic = cardLogic.gameManager.currentFocusCardLogic;
        int effectAmount = subEffect.EffectAmount;
        int duration = subEffect.duration;
        EffectsUsed effectsUsed = subEffect.effectUsed;
        List<CardStatus> statuses = new();

        switch (effectsUsed)
        {
            case EffectsUsed.Rally:
                combatantLogic.StatAdjustment(effectAmount, Status.AtkGain);
                break;
            case EffectsUsed.Damage:
                combatantLogic.TakeDamage(effectAmount, false);
                break;
            case EffectsUsed.Regeneration:
                combatantLogic.Heal(effectAmount);
                break;
            case EffectsUsed.Vigor:
                combatantLogic.StatAdjustment(effectAmount, Status.AtkGain);
                combatantLogic.StatAdjustment(effectAmount, Status.HpGain);
                break;
            case EffectsUsed.Terrify:
                combatantLogic.StatAdjustment(effectAmount, Status.HpLoss);
                combatantLogic.StatAdjustment(effectAmount, Status.AtkLoss);
                break;
            case EffectsUsed.Intimidate:
                combatantLogic.StatAdjustment(effectAmount, Status.AtkLoss);
                break;
            case EffectsUsed.Weaken:
                combatantLogic.StatAdjustment(effectAmount, Status.HpLoss);
                break;
            case EffectsUsed.Shatter:
                monsterLogic.MonsterDeath();
                break;
            case EffectsUsed.Taunt:
                Taunt taunt = new(logic, cardLogic, duration);
                combatantLogic.SetTargetStatus(taunt, TargetState.Taunt);
                break;
            case EffectsUsed.Stealth:
                Stealth stealth = new(logic, cardLogic, duration);
                combatantLogic.SetTargetStatus(stealth, TargetState.Stealth);
                break;
            //everything above here works fine
            case EffectsUsed.Camouflage:
                Camouflage camouflage = new(logic, cardLogic, duration);
                combatantLogic.SetTargetStatus(camouflage, TargetState.Stealth);
                break;
            //everything from here works fine
            case EffectsUsed.Armor:
                Armor armor = new(logic, cardLogic, effectAmount, duration);
                combatantLogic.AddNonStackingBuff(armor);
                break;
            case EffectsUsed.Barrier:
                Barrier barrier = new(logic, cardLogic, effectAmount, duration);
                combatantLogic.AddNonStackingBuff(barrier);
                break;
            //till here
            case EffectsUsed.Sleep:
                Sleep sleep = new(logic, cardLogic, duration);
                combatantLogic.AddNonStackingDebuff(sleep);
                break;
            case EffectsUsed.Stun:
                Stun stun = new(logic, cardLogic, duration);
                combatantLogic.AddNonStackingDebuff(stun);
                break;
            case EffectsUsed.Provoke:
                Provoke provoke = new(logic, cardLogic, duration);
                combatantLogic.SetTargetStatus(provoke, TargetState.Stealth);
                break;
            case EffectsUsed.Disarm:
                Disarm disarm = new(logic, cardLogic, duration);
                combatantLogic.AddNonStackingDebuff(disarm);
                break;
            //everything from here works fine
            case EffectsUsed.Burn:
                //burns have a default timer of two turns, if duration is set to 0/not defined(int), default applies
                Burn burn = new(logic, cardLogic, duration);
                combatantLogic.cardStatuses.Add(burn);
                GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(burn);
                break;
            case EffectsUsed.Poison:
                //poisons have a default timer of four turns, if duration is set to 0/not defined(int), default applies
                Poison poison = new(logic, cardLogic, duration);
                combatantLogic.cardStatuses.Add(poison);
                GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(poison);
                break;
            case EffectsUsed.Bomb:
                //bombs have a default timer of three turns, if duration is set to 0/not defined(int), default applies
                Bomb bomb = new(logic, cardLogic, duration);
                combatantLogic.cardStatuses.Add(bomb);
                GetComponent<MonsterLogic>().currentSlot.SetStatusIcon(bomb);
                break;
            //till here
            case EffectsUsed.Spot:
                Spot spot = new(logic, cardLogic, duration);
                combatantLogic.AddNonStackingDebuff(spot);
                break;
            //this works
            case EffectsUsed.Bounce:
                StartCoroutine(monsterLogic.BounceCard());
                break;
            case EffectsUsed.Detonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    statuses.Add(status);
                foreach (CardStatus status in statuses)
                    status.DetonateActions(cardLogic.gameManager);
                break;
            case EffectsUsed.BombDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Bomb)
                        statuses.Add(status);
                foreach (CardStatus status in statuses)
                    status.DetonateActions(cardLogic.gameManager);
                break;
            case EffectsUsed.BurnDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Burn)
                        statuses.Add(status);
                foreach (CardStatus status in statuses)
                    status.DetonateActions(cardLogic.gameManager);
                break;
            case EffectsUsed.PoisonDetonate:
                foreach (CardStatus status in combatantLogic.cardStatuses)
                    if (status is Poison)
                        statuses.Add(status);
                foreach (CardStatus status in statuses)
                    status.DetonateActions(cardLogic.gameManager);
                break;
            case EffectsUsed.BuffDispel:
                for (int i = effectAmount; i > 0;)
                {
                    if (combatantLogic.cardStatuses.Count > 0)
                    {
                        CardStatus status = combatantLogic.BuffCheck(Buffs.Undefined);
                        if (status != null)
                            combatantLogic.RemoveCardStatus(status);
                    }
                    i--;
                }
                break;
            case EffectsUsed.DebuffDispel:
                for (int i = effectAmount; i > 0;)
                {
                    if (combatantLogic.cardStatuses.Count > 0)
                    {
                        CardStatus status = combatantLogic.DebuffCheck(Debuffs.Undefined);
                        if (status != null)
                            combatantLogic.RemoveCardStatus(status);
                    }
                    i--;
                }
                break;
            case EffectsUsed.Silence:
                Silence silence = new(logic, cardLogic, duration);
                combatantLogic.AddNonStackingDebuff(silence);
                break;
            case EffectsUsed.BloodBoost:
                break;
            case EffectsUsed.Advancement:
                break;
            case EffectsUsed.Knockback:
                break;
            case EffectsUsed.Pull:
                break;
            case EffectsUsed.Shift:
                break;
            case EffectsUsed.Immortality:
                break;
            case EffectsUsed.Immunity:
                break;
            case EffectsUsed.Spin:
                break;
            case EffectsUsed.Discard:
                break;
            case EffectsUsed.Mill:
                break;
            case EffectsUsed.ForceRetreat:
                break;
            case EffectsUsed.Return:
                break;
            case EffectsUsed.Retreat:
                break;
            case EffectsUsed.DeathCurse:
                break;
            case EffectsUsed.Mark:
                break;
            case EffectsUsed.Freeze:
                break;
            default:
                throw new MissingReferenceException("effect not found");
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

    private void EffectResolutionAfterAnimation(SubEffect subEffect)
    {
        EffectsUsed effectUsed = subEffect.effectUsed;
        int effectAmount = subEffect.effectAmount;
        cardLogic.SetFocusCardLogic();
        EffectLogger(subEffect, cardLogic.targetingLogic.targets);
        List<CardLogic> tempTargets = new(cardLogic.targetingLogic.targets);

        switch (effectUsed)
        {
            //for effects that need special targeting
            case EffectsUsed.Target:
                TargetEffectLogic(subEffect);
                break;
            case EffectsUsed.BloodCost:
                List<Attunement> sendList = new()
                {
                    Enum.Parse<Attunement>(subEffect.TargetStats[0])
                };
                cardLogic.dataLogic.cardController.BloodLoss(sendList, effectAmount);
                break;
            //effects that access game manager methods, can be optimized further
            case EffectsUsed.Reinforce:
                StartCoroutine(cardLogic.gameManager.DrawCard(effectAmount, cardLogic.dataLogic.cardController));
                return;
            case EffectsUsed.BloodRecovery:
                cardLogic.dataLogic.cardController.BloodGain(Attunement.Untuned, effectAmount);
                break;
            case EffectsUsed.Recruit:
                StartCoroutine(cardLogic.gameManager.SearchCard(tempTargets, cardLogic));
                return;
            case EffectsUsed.Recover:
                foreach (CardLogic target in tempTargets)
                    StartCoroutine(cardLogic.gameManager.RecoverCard(target, cardLogic));
                return;

            //these are undefined effects
            case EffectsUsed.Damage:
            case EffectsUsed.Regeneration:
            case EffectsUsed.Shatter:
            case EffectsUsed.Detonate:
            case EffectsUsed.BurnDetonate:
            case EffectsUsed.BombDetonate:
            case EffectsUsed.PoisonDetonate:
            case EffectsUsed.Bounce:
                foreach (CardLogic target in tempTargets)
                    target.EffectHandler(subEffect);
                break;

            //these are buffs
            case EffectsUsed.Rally:
            case EffectsUsed.Vigor:
            case EffectsUsed.Taunt:
            case EffectsUsed.Stealth:
            case EffectsUsed.Armor:
            case EffectsUsed.Camouflage:
            case EffectsUsed.Barrier:
                foreach (CardLogic target in tempTargets)
                    target.EffectHandler(subEffect);
                break;

            //these are debuffs
            case EffectsUsed.Terrify:
            case EffectsUsed.Intimidate:
            case EffectsUsed.Weaken:
            case EffectsUsed.Sleep:
            case EffectsUsed.Stun:
            case EffectsUsed.Provoke:
            case EffectsUsed.Disarm:
            case EffectsUsed.Burn:
            case EffectsUsed.Poison:
            case EffectsUsed.Bomb:
            case EffectsUsed.Spot:
                foreach (CardLogic target in tempTargets)
                    target.EffectHandler(subEffect);
                break;
            //these are unedefined effects
            case EffectsUsed.FreeRevive:
            case EffectsUsed.Revive:
            case EffectsUsed.FreeDeploy:
            case EffectsUsed.Deploy:
                foreach (CardLogic target in tempTargets)
                    target.GetComponent<PlayableLogic>().PlayCard(effectUsed, cardLogic.dataLogic.cardController);
                break;
            default:
                throw new MissingReferenceException("Attempting to use an unimplemented effect");
        }
        if (!cardLogic.gameManager.isWaitingForResponse)
            FinishResolution(subEffect);
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
