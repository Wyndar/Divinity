using System.Collections;
using System.Collections.Generic;
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
        isPerformingAction = false;
        turnManager.TriggerPhaseChange();
    }

    //indiscriminate attack spam
    public void BattlePhase()
    {
        if (isPerformingAction == true)
            return;
        foreach (CardLogic cardLogic in gm.turnPlayer.fieldLogicList)
        {
            isPerformingAction = true;
cardLogic.GetComponent<CombatantLogic>().DeclareAttack();
        }
        turnManager.TriggerPhaseChange();
        isPerformingAction = false;
    }

    private void PlayLegalCard()
    {
        CardLogic highestAttack = BestAtkSort(AIPlayer.playableLogicList);
        CardLogic highestHp = BestHpSort(AIPlayer.playableLogicList);
        if (highestHp == null && highestAttack == null)
            return;
        isPerformingAction = true;
        if (highestHp.GetComponent<CombatantLogic>().currentAtk >= highestAttack.GetComponent<CombatantLogic>().currentAtk)
            highestHp.GetComponent<PlayableLogic>().PlayCard("deploy", false, AIPlayer);
        else
            highestAttack.GetComponent<PlayableLogic>().PlayCard("deploy", false, AIPlayer);
    }

    //for now, just using random targets. will write logic later
    public void GetEffectTarget()
    {
        gm.currentFocusCardLogic.RandomTargetAcquisition(gm.currentFocusCardLogic.effectCountNumber, gm.currentFocusCardLogic.subCountNumber);
    }

    private void UseLegalEffects()
    {
        foreach(CardLogic cardLogic in AIPlayer.canUseEffectLogicList)
        {
            foreach (Effect effect in cardLogic.effects)
            {
                if (!effect.EffectUsed.Contains("Deployed"))
                    continue;
                Debug.Log(cardLogic.cardName);
                isPerformingAction = true;
                int subNum = effect.EffectUsed.FindIndex(a => a == "Deployed");
                int effNum = cardLogic.effects.FindIndex(a => a == effect);
                cardLogic.EffectActivation(effNum, subNum);
            }
        }
    }

    public bool UseShield(int damage)
    {
        if (BestAtkSort(AIPlayer.enemy.canAttackLogicList).GetComponent<CombatantLogic>().currentAtk <= damage)
            return true;
        if (AIPlayer.heroCardLogic.GetComponent<CombatantLogic>().currentHp <= damage)
            return true;
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
            cardLogic.TryGetComponent<CombatantLogic>(out CombatantLogic combatantLogic);
            if (combatantLogic == null)
                continue;
            if (highestAtk > combatantLogic.currentAtk)
                continue;
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
            cardLogic.TryGetComponent<CombatantLogic>(out CombatantLogic combatantLogic);
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