using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public GameBattleManager gm;
    public PlayerManager AIPlayer;
    public TurnManager turnManager;
    public bool isPerformingAction;

    private Coroutine previousCoroutine;
    private Coroutine currentCoroutine;

    public IEnumerator Decision()
    {
        previousCoroutine = currentCoroutine;
        currentCoroutine = null;
        isPerformingAction = true;
        AIPlayer.hourglassIcon.SetActive(true);
        yield return new WaitForSeconds(1f);

        AIPlayer.hourglassIcon.SetActive(false);
        if (gm.currentPhase == Phase.MainPhase && gm.turnPlayer == AIPlayer)
        {
            currentCoroutine = StartCoroutine(MainPhase());
            yield break;
        }
        if (gm.currentPhase == Phase.BattlePhase && gm.turnPlayer == AIPlayer)
        {
            currentCoroutine = StartCoroutine(BattlePhase());
            yield break;
        }
        //implement response to opponent action here
        isPerformingAction = false;
        yield break;
    }

    public void MakeDecision()
    {
        if (isPerformingAction)
            return;
        StopAllCoroutines();
        currentCoroutine = StartCoroutine(Decision());
    }

    //indiscriminate summon of best stats and effect activation, work is needed here
    public IEnumerator MainPhase()
    {
        if (previousCoroutine != null)
            StopCoroutine(previousCoroutine);
        //idgi but we need to check again here or it breaks chains 
        if(gm.isActivatingEffect)
            yield break;
        previousCoroutine = currentCoroutine;
        currentCoroutine = null;
        if (AIPlayer.costCount > 0 && AIPlayer.playableLogicList.Count > 0)
        {
            PlayLegalCard();
            yield break;
        }
        if (AIPlayer.canUseEffectLogicList.Count > 0)
        {
            UseLegalEffects();
            yield break;
        }
        if (MovedMonster())
            yield return null;
        isPerformingAction = false;
        yield return new WaitWhile(() => gm.activationChainList.Count > 0 || gm.gameState != GameState.Open || gm.isPlayingCard || gm.isActivatingEffect);
        turnManager.TriggerPhaseChange();
        yield break;
    }

    private bool MovedMonster()
    {
        foreach (CardLogic cardLogic in AIPlayer.fieldLogicList)
        {
            if (cardLogic is not MonsterLogic monster)
                continue;
            if (monster.hasMoved)
                continue;
            foreach (CardSlot cardSlot in AIPlayer.cardSlots)
            {
                if (Mathf.Abs(monster.currentSlot.row - cardSlot.row) > 1 || Mathf.Abs(monster.currentSlot.column - cardSlot.column) > 1
                    || cardSlot == monster.currentSlot || cardSlot.cardInZone != null || cardSlot.controller != AIPlayer)
                    continue;
                bool isBlocked = false;
                if (monster.combatLogic.currentAtk == 0 && cardSlot.isFrontline)
                    continue;
                if (monster.combatLogic.currentAtk > 0 && cardSlot.isFrontline)
                    foreach (CardLogic enemy in AIPlayer.enemy.fieldLogicList)
                        if (enemy.GetComponent<MonsterLogic>().currentSlot.column == cardSlot.column)
                        {
                            isBlocked = true;
                            break;
                        }
                if (isBlocked)
                    continue;
                monster.Move(cardSlot);
                return true;
            }
        }
        return false;
    }

    //indiscriminate attack spam
    public IEnumerator BattlePhase()
    {
        if (previousCoroutine != null)
            StopCoroutine(previousCoroutine);
        previousCoroutine = currentCoroutine;
        currentCoroutine = null;

        if (AIPlayer.canAttackLogicList.Count > 0)
        {
            CombatantLogic combatant = AIPlayer.canAttackLogicList[0].GetComponent<CombatantLogic>();
            combatant.DeclareAttack();
            yield break;
        }
        else
            isPerformingAction = false;
        yield return new WaitWhile(() => gm.activationChainList.Count > 0 || gm.gameState != GameState.Open || gm.isPlayingCard || gm.isActivatingEffect);
        turnManager.TriggerPhaseChange();
        yield break;
    }

    private void PlayLegalCard()
    {
        CardLogic cardToPlay = BestAtkSort(AIPlayer.playableLogicList) ?? BestCostSort(AIPlayer.playableLogicList);
        List<int> blockedColumns = new();
        foreach (MonsterLogic logic in AIPlayer.enemy.fieldLogicList)
            if (!blockedColumns.Contains(logic.currentSlot.column))
                blockedColumns.Add(logic.currentSlot.column);
        if (blockedColumns.Count < 4)
            foreach (CardSlot slot in AIPlayer.cardSlots)
            {
                if (blockedColumns.Contains(slot.column) || slot.isFrontline || slot.cardInZone != null)
                    continue;
                if ((slot.column == 1 || slot.column == 4) && (!blockedColumns.Contains(2) || !blockedColumns.Contains(3)))
                    continue;
                gm.currentFocusCardSlot = slot;
            }
        cardToPlay.GetComponent<PlayableLogic>().PlayCard(EffectsUsed.Deploy, AIPlayer);
    }

    //for now, just using random targets. will write logic later
    public void GetEffectTarget()
    {
        gm.currentFocusCardLogic.targetingLogic.RandomTargetAcquisition(gm.currentFocusCardLogic.effectLogic.focusSubEffect);
    }

    private void UseLegalEffects()
    {
        if (AIPlayer.canUseEffectLogicList.Count == 0)
            return;
        if (gm.isActivatingEffect)
            return;
        if (gm.isPlayingCard)
            return;
        CardLogic cardLogic = AIPlayer.canUseEffectLogicList[0];
        SubEffect subEffect = AIPlayer.canUseSubEffectList[0];
        cardLogic.EffectActivation(subEffect);
    }

    public bool UseShield(int damage, bool wasAttack)
    {
        //use shield if about to die, highest priority
        if (AIPlayer.heroCardLogic.GetComponent<CombatantLogic>().currentHp <= damage)
            return true;
        //ignore damage less than a fifth of current hp
        if (damage < AIPlayer.heroCardLogic.GetComponent<CombatantLogic>().currentHp / 5)
            return false;
        //anything else if it's not an attack is a bust,do NOT take the damage
        if (!wasAttack)
            return true;
        //use shield if about to get hit by highest atk otherwise
        if (BestAtkSort(AIPlayer.enemy.canAttackLogicList).GetComponent<CombatantLogic>().currentAtk == damage)
            return true;
        //else failsafe default
        return false;
    }

    //for now always use
    public bool ActivateOptionalEffect()
    {
        return true;
    }

    private CardLogic BestAtkSort(List<CardLogic> sortList)
    {
        int highestAtk = -1;
        CardLogic bestStats = null;
        foreach (CardLogic cardLogic in sortList)
        {
            cardLogic.TryGetComponent(out CombatantLogic combatantLogic);
            //ignore non combatants
            if (combatantLogic == null)
                continue;
            //ignore lower atk than highest
            if (highestAtk > combatantLogic.currentAtk)
                continue;
            //in event of a tie, use higher hp as fallback
            if (highestAtk == combatantLogic.currentAtk)
                if (bestStats.GetComponent<CombatantLogic>().maxHp > combatantLogic.maxHp)
                    continue;
            //if hp of new card is higher or tied, it's safe to swap...
            highestAtk = combatantLogic.currentAtk;
            bestStats = cardLogic;
        }
        return bestStats;
    }

    //idk why but lets keep this for now
    private CardLogic BestHpSort(List<CardLogic> sortList)
    {
        int highestHP = -1;
        CardLogic bestStats = null;
        foreach (CardLogic cardLogic in sortList)
        {
            cardLogic.TryGetComponent(out CombatantLogic combatantLogic);
            if (combatantLogic == null)
                continue;
            if (highestHP >= combatantLogic.maxHp)
                continue;
            highestHP = combatantLogic.maxHp;
            bestStats = cardLogic;
        }
        return bestStats;
    }

    private CardLogic BestCostSort(List<CardLogic> sortList)
    {
        int highestCost = -1;
        CardLogic bestStats = null;
        foreach (CardLogic cardLogic in sortList)
        {
            cardLogic.TryGetComponent(out PlayableLogic playableLogic);
            if (playableLogic == null)
                continue;
            if (highestCost >= playableLogic.cost)
                continue;
            highestCost = playableLogic.cost;
            bestStats = cardLogic;
        }
        return bestStats;
    }
}