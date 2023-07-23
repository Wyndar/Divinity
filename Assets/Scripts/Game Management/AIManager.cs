using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static Game_Manager;

public class AIManager : MonoBehaviour
{
    public Game_Manager gm;
    public PlayerManager AIPlayer;
    public TurnManager turnManager;
    public bool isPerformingAction;

    public IEnumerator Decision()
    {
        isPerformingAction = true;
        AIPlayer.hourglassIcon.SetActive(true);
        yield return new WaitForSeconds(1f);

        AIPlayer.hourglassIcon.SetActive(false);
        if (gm.currentPhase == Phase.MainPhase && gm.turnPlayer == AIPlayer)
        {
            MainPhase();
            yield break;
        }
        if (gm.currentPhase == Phase.BattlePhase && gm.turnPlayer == AIPlayer)
        {
            BattlePhase();
            yield break;
        }
        //implement response to opponent action here
        isPerformingAction = false;
        yield break;
    }

    public void MakeDecision()
    {
        if (isPerformingAction == true)
            return;
        StartCoroutine(Decision());
    }

    //indiscriminate summon of best stats and effect activation, work is needed here
    public void MainPhase()
    {
        if (AIPlayer.costCount > 0 && AIPlayer.playableLogicList.Count > 0)
            PlayLegalCard();
        if (AIPlayer.canUseEffectLogicList.Count > 0)
            UseLegalEffects();
        else
            isPerformingAction = false;
        if (gm.gameState != GameState.Open)
            return;
        turnManager.TriggerPhaseChange();
    }

    //indiscriminate attack spam
    public void BattlePhase()
    {
        if (AIPlayer.canAttackLogicList.Count > 0)
        {
            CombatantLogic combatant = AIPlayer.canAttackLogicList[0].GetComponent<CombatantLogic>();
            combatant.DeclareAttack();
        }
        else
            isPerformingAction = false;
        if (gm.gameState != GameState.Open)
            return;
        turnManager.TriggerPhaseChange();
    }

    private void PlayLegalCard()
    {
        CardLogic cardToPlay = BestAtkSort(AIPlayer.playableLogicList);
        //if no fighters, play a spell
        if (cardToPlay == null)
            cardToPlay = BestCostSort(AIPlayer.playableLogicList);
        cardToPlay.GetComponent<PlayableLogic>().PlayCard("deploy", false, AIPlayer);
    }

    //for now, just using random targets. will write logic later
    public void GetEffectTarget()
    {
        gm.currentFocusCardLogic.RandomTargetAcquisition(gm.currentFocusCardLogic.effectCountNumber, gm.currentFocusCardLogic.subCountNumber);
    }

    private void UseLegalEffects()
    {
        if (AIPlayer.canUseEffectLogicList.Count == 0)
            return;
        if (gm.isActivatingEffect)
            return;
        CardLogic cardLogic = AIPlayer.canUseEffectLogicList[0];
        int effNum = AIPlayer.canUseEffectNumber[0];
        int subNum = AIPlayer.canUseEffectSubNumber[0];
        cardLogic.EffectActivation(effNum, subNum);
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