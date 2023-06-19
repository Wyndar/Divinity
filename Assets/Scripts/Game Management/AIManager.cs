using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AIManager : MonoBehaviour
{
    public Game_Manager gm;
    public PlayerManager AIPlayer;
    public TurnManager turnManager;
    public bool isPerformingAction;

    public void MakeDecision()
    {
        if (isPerformingAction == true)
            return;
        if (gm.currentPhase == Game_Manager.Phase.MainPhase)
            MainPhase();
        if (gm.currentPhase == Game_Manager.Phase.BattlePhase)
            BattlePhase();
    }

    //indiscriminate summon of best stats and effect activation, work is needed here
    public void MainPhase()
    {
        if (isPerformingAction == true)
            return;
        if (AIPlayer.costCount > 0 && AIPlayer.playableLogicList.Count > 0)
            PlayLegalCard();
        if (AIPlayer.canUseEffectLogicList.Count > 0)
            UseLegalEffects();
        if (gm.gameState != Game_Manager.GameState.Open)
            return;
        turnManager.TriggerPhaseChange();
    }

    //indiscriminate attack spam
    public void BattlePhase()
    {
        if (isPerformingAction == true)
            return;
        if (AIPlayer.canAttackLogicList.Count > 0)
        {
            CombatantLogic combatant = AIPlayer.canAttackLogicList[0].GetComponent<CombatantLogic>();
            isPerformingAction = true;
            combatant.DeclareAttack();
        }
        if (gm.gameState != Game_Manager.GameState.Open)
            return;
        turnManager.TriggerPhaseChange();
       
    }

    private void PlayLegalCard()
    {
        CardLogic highestAttack = BestAtkSort(AIPlayer.playableLogicList);
        if (highestAttack == null)
            return;
        isPerformingAction = true;
        highestAttack.GetComponent<PlayableLogic>().PlayCard("deploy", false, AIPlayer);
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
        isPerformingAction = true;
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
        //anything else if it's an attack is a bust, take the damage
        if (wasAttack)
            return false;
        //use shield if about to get hit by highest atk otherwise
        if (BestAtkSort(AIPlayer.enemy.canAttackLogicList).GetComponent<CombatantLogic>().currentAtk == damage)
            return true;
        //else if not attack and nothing else
        return false;
    }

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
}