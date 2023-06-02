using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CardLogic : MonoBehaviour
{
    public Game_Manager GameManager;

    public EnumConverter enumConverter;

    public PlayerManager cardOwner;
    public Transform cardFace, cardBack, cardImage, cardOutline;
    public Sprite image;

    public string id, cardName, cardType, cardText, flavorText;

    public bool isFaceDown, isTargeted, hasGottenTargets;

    public enum Location
    {
        Deck,HeroDeck,Hand,Field,Grave,Limbo,Outside,Undefined
    }

    public Location currentLocation;
    public int locationOrderNumber;

    public List<CardLogic> targets = new();
    public List<CardLogic> validTargets = new();
    public List<CardLogic> scalers = new();
    public List<CardLogic> validScalers = new();
    public List<Effect> effects;
    public List<int> currentActivations = new();
    public int effectCountNumber;
    public int subCountNumber;

    private IEnumerator ActivationCoroutine(int effectNumber, int subEffectNumber)
    {
        GameManager.StateChange(Game_Manager.GameState.EffectActivation);
        GameManager.DisableRayBlocker();
        Vector3 originalPosition = transform.localPosition;
        transform.position = cardOwner.activationZone.position;
        Vector3 originalScale = this.transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        EffectActivationAfterAnimation(effectNumber, subEffectNumber);
        yield break;
    }

    private IEnumerator ResolutionCoroutine(int effectNumber, int subEffectNumber)
    {
        GameManager.DisableRayBlocker(); GameManager.StateChange(Game_Manager.GameState.EffectResolution);
        Vector3 originalPosition = transform.localPosition;
        transform.position = cardOwner.activationZone.position;
        Vector3 originalScale = this.transform.localScale;
        transform.localScale = new Vector3(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        EffectResolutionAfterAnimation(effectNumber, subEffectNumber);
        yield break;
    }

    public List<CardLogic> GetValidTargets(int effectNumber, int subEffectNumber)
    {
        List<CardLogic> allTargetsList = new List<CardLogic>(FindObjectsOfType<CardLogic>());
        List<CardLogic> returnList = new();
        for (int i = 0; i< allTargetsList.Count; i++)
        {
            CardLogic target = allTargetsList[i];
            Effect targetingEffect = effects[effectNumber];
            allTargetsList[i].TryGetComponent<CombatantLogic>(out var combatantStats);
            allTargetsList[i].TryGetComponent<PlayableLogic>(out var playableStats);
            // basic targeting requirements... don't target wrong location, don't target wrong owner or wrong card types
            if (target == this && targetingEffect.AllowSelfTarget[subEffectNumber] == false)
                continue;
            if (targetingEffect.TargetLocation != null && target.currentLocation != enumConverter.LocationStringToEnum(targetingEffect.TargetLocation[subEffectNumber]) && targetingEffect.TargetLocation[subEffectNumber] != "any")
                continue;
            if (target.cardOwner == cardOwner && targetingEffect.EffectTargetOwner[subEffectNumber] == "Opponent")
                continue;
            if (target.cardOwner != cardOwner && targetingEffect.EffectTargetOwner[subEffectNumber] == "Player")
                continue;
            if (targetingEffect.EffectTargetType != null && target.cardType != targetingEffect.EffectTargetType[subEffectNumber] && targetingEffect.EffectTargetType[subEffectNumber] != "any" && targetingEffect.EffectTargetType[subEffectNumber] !="combatant")
                continue;
            if (targetingEffect.EffectTargetType != null && targetingEffect.EffectTargetType[subEffectNumber] == "combatant" && target.cardType == "spell")
                continue;
            // don't target max hp with healing effects
            if (targetingEffect.EffectUsed[subEffectNumber] == "Regeneration" && (combatantStats == null || combatantStats.maxHp == combatantStats.currentHp))
                continue;
            // don't target no hp with damage effects
            if (targetingEffect.EffectUsed[subEffectNumber] == "Damage" && (combatantStats == null || combatantStats.currentHp <= 0))
                continue;

            if (targetingEffect.TargetStat != null)
            {
                string checkedStat = targetingEffect.TargetStat[subEffectNumber];
                string condition = targetingEffect.TargetStatCondition[subEffectNumber];
                int amount = targetingEffect.TargetStatAmount[subEffectNumber];
                int stat = 0;
                //as more types of effects are added, more checks will be needed
                if (checkedStat == "none")
                {
                    returnList.Add(target);
                    continue;
                }
                if (checkedStat == "current atk")
                {
                    if (combatantStats == null)
                        continue;
                    stat = combatantStats.currentAtk;
                }

                if(checkedStat == "cost")
                {
                    if (playableStats == null)
                        continue;
                    stat = playableStats.cost;
                }

                // don't target stats not equal requirements
                if (condition == "==" && stat != amount)
                    continue;
                if (condition == ">=" && stat < amount)
                    continue;
                if (condition == "<=" && stat > amount)
                    continue;
                if (condition == ">" && stat <= amount)
                    continue;
                if (condition == "<" && stat >= amount)
                    continue;
            }
            returnList.Add(target);
        }
        if (returnList.Count < 1)
            Debug.Log("No valid targets");
        return returnList;
    }

    public void EffectActivation(int countNumber, int subCount)=>StartCoroutine(ActivationCoroutine(countNumber,subCount));

    private void EffectActivationAfterAnimation(int countNumber, int subCount)
    {
        Effect activatingEffect = effects[countNumber];
        switch (activatingEffect.EffectType[subCount])
        {
            case "Deployment":
                TargetCheck(countNumber, subCount);
                break;
            case "Chain":
                TargetCheck(countNumber, subCount);
                break;
            case "Deployed":
                if (currentLocation == enumConverter.LocationStringToEnum(activatingEffect.ActivationLocation))
                {
                    TargetCheck(countNumber, subCount);
                }
                break;
        }
        GameManager.currentFocusCardLogic = this;
    }

    public void EffectResolution(int countNumber, int subCount) =>StartCoroutine(ResolutionCoroutine(countNumber, subCount));

    private void EffectResolutionAfterAnimation(int countNumber, int subCount)
    {
        Effect resolvingEffect = effects[countNumber];
       
        switch (resolvingEffect.EffectUsed[subCount])
        {
            case "Reinforce":
                GameManager.DrawCard(resolvingEffect.EffectAmount[subCount], cardOwner);
                break;
            case "Damage":
                for (int targetCountNumber = 0; targetCountNumber < targets.Count; targetCountNumber++)
                {
                    if (targets[targetCountNumber] == null)
                        break;
                    targets[targetCountNumber].gameObject.GetComponent<CombatantLogic>().TakeDamage(resolvingEffect.EffectAmount[subCount]);
                }
                break;
            case "Regeneration":
                for(int i = 0; i<targets.Count;i++)
                {
                    if (targets[i] == null)
                        continue;
                    targets[i].GetComponent<CombatantLogic>().Heal(resolvingEffect.EffectAmount[subCount]);
                }
                break;
            case "Rally":
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                        continue;
                    targets[i].GetComponent<CombatantLogic>().ATKAdjustment(1, true);
                }
                break;
            case "Free Revive":
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                        continue;
                    targets[i].GetComponent<PlayableLogic>().PlayCard("revive", true);
                }
                    break;
            case "Recruit":
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                        continue;
                    GameManager.SearchCard(targets[i], targets[i].cardOwner);
                }
                break;
            case "Free Deploy":
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                        continue;
                    targets[i].GetComponent<PlayableLogic>().PlayCard("deploy", true);
                }
                break;
            case "Target":
                string checkedStat = resolvingEffect.TargetStat[subCount];
                targets[0].TryGetComponent<CombatantLogic>(out var combatantStats);
                targets[0].TryGetComponent<PlayableLogic>(out var playableStats);
                if (checkedStat == "current atk")
                {
                    resolvingEffect.EffectAmount[subCount + 1] = combatantStats.currentAtk;
                }

                if (checkedStat == "cost")
                {
                    resolvingEffect.EffectAmount[subCount + 1] = playableStats.cost;
                }
                break;
            case "Vigor":
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                        continue;
                    targets[i].GetComponent<CombatantLogic>().ATKAdjustment(resolvingEffect.EffectAmount[subCount], true);
                    targets[i].GetComponent<CombatantLogic>().MaxHPAdjustment(resolvingEffect.EffectAmount[subCount], true);
                }
                break;
            case "Intimidate":
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                        continue;
                    targets[i].GetComponent<CombatantLogic>().ATKAdjustment(resolvingEffect.EffectAmount[subCount], false);
                }
                break;
            case "Shatter":
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                        continue;
                    targets[i].GetComponent<MonsterLogic>().MonsterDeath();
                }
                break;
            case "Blood Recovery":
                cardOwner.costCount += resolvingEffect.EffectAmount[subCount];
                break;

        }

        if (resolvingEffect.MaxActivations != 0 && currentActivations[countNumber] != 0)
            currentActivations[countNumber] -= 1;

        //chainlist for effect triggers
        GameManager.GetEffectTriggers(countNumber, subCount, this);

       if( ResolveSubsequentSubeffects(countNumber, subCount))
        {
            if (targets != null)
                targets.Clear();
            if (validTargets != null)
                validTargets.Clear();
            if (cardType == "spell")
                gameObject.GetComponent<PlayableLogic>().MoveToGrave();

            //resolve chain after all possible effect chains are linked
            GameManager.ChainResolution();
        };
    }

    public bool ResolveSubsequentSubeffects(int countNumber, int subCount)
    {
        Effect resolvingEffect = effects[countNumber];

        //resolve subsequent subeffects in the same effect if there is any
        if (resolvingEffect.EffectUsed.Count <= subCount + 1)
            return true;
        if (resolvingEffect.EffectType[subCount] != resolvingEffect.EffectType[subCount + 1])
            return true;
        if (resolvingEffect.EffectActivationIsMandatory[subCount + 1] == false)
        {
            effectCountNumber = countNumber;
            subCountNumber = subCount + 1;
            GameManager.EnableActivationPanel();
            return false;
        }
        //dependent on targets of previous effect
        if (resolvingEffect.EffectTargetAmount[subCount + 1] == 99)
        {
            EffectResolution(countNumber, subCount + 1);
        }
        else
        {
            if (targets != null)
                targets.Clear();
            if (validTargets != null)
                validTargets.Clear();
            TargetCheck(countNumber, subCount + 1);
        }
        return false;
    }

    public void TargetCheck(int countNumber, int subCount)
    {
        Effect targetingEffect = effects[countNumber];
        if (targetingEffect.EffectTargetAmount == null)
        {
            hasGottenTargets = true;
            EffectResolution(countNumber, subCount);
            return;
        }
        if (targetingEffect.EffectTargetAmount[subCount] == 0)
        {
            hasGottenTargets = true;
            EffectResolution(countNumber, subCount);
            return;
        }

        if (targets == null || targets.Count < targetingEffect.EffectTargetAmount[subCount])
        {
            GameManager.StateChange(Game_Manager.GameState.Targeting);
            validTargets = new(GetValidTargets(countNumber, subCount));
            if (targetingEffect.TargetingType[subCount] == "manual")
            {
                //if no valid targets, end effect
                if (validTargets.Count == 0)
                {
                    GameManager.StateReset();
                    return;
                }
                effectCountNumber = countNumber;
                subCountNumber = subCount;
                if (targetingEffect.TargetLocation[subCount] != "Field")
                    GameManager.EnableCardScrollScreen(validTargets);
            }
            if (targetingEffect.TargetingType[subCount] == "auto")
            {
                AutoTargetAcquisition(countNumber, subCount);
                return;
            }
            if (targetingEffect.TargetingType[countNumber] == "enemy")
            {
                Debug.Log("Need to implement AI");
                return;
            }
        }
        if (GameManager.gameState != Game_Manager.GameState.Targeting && targets.Count == 0)
        {
            hasGottenTargets = true;
            EffectResolution(countNumber, subCount);
            return;
        }
    }

    //called by ux manager clicked GameObject method with current focus card logic count and subcount
    public void ManualTargetAcquisition(int countNumber, int subCount)
    {
        if (GameManager.gameState == Game_Manager.GameState.Targeting)
        {
            CardLogic targeter = GameManager.currentFocusCardLogic;
            for (int j = 0; j < targeter.validTargets.Count; j++)
            {
                if (targeter.validTargets[j] != this)
                    continue;
                if (targeter.targets == null)
                    targeter.targets = new ();
                targeter.targets.Add(this);
            }
            //if you hit the needed amount of targets or all valid targets are taken, resolve
            if (targeter.targets.Count == targeter.effects[countNumber].EffectTargetAmount[subCount] || targeter.targets.Count == targeter.validTargets.Count)
            {
                GameManager.StateReset();
                targeter.EffectResolution(countNumber, subCount);
                return;
            }
        }
    }

    //less auto more all target
    public void AutoTargetAcquisition(int countNumber, int subCount)
    {
        targets = new(validTargets);
        EffectResolution(countNumber, subCount);
        return;
    }

    public void OptionalEffectResolution()
    {
        //if you need the targets from previous effect to resolve
        if (effects[effectCountNumber].TargetingType != null && effects[effectCountNumber].EffectTargetAmount[subCountNumber] == 99)
            EffectResolution(effectCountNumber, subCountNumber);
        else
        {
            targets.Clear();
            validTargets.Clear();
            EffectActivation(effectCountNumber, subCountNumber);
        }
        //otherwise activate effect afresh
    }

    //reset activation count for effects usually at turn start
    public void EffectRefresh()
    {
        currentActivations.Clear();
        foreach (Effect effect in effects)
            currentActivations.Add(effect.MaxActivations);
    }

    public void FlipFaceUp()
    {
        isFaceDown = false;
        cardBack.gameObject.SetActive(false);
        cardFace.gameObject.SetActive(true);
        cardImage.gameObject.SetActive(true);
    }

    public void FlipFaceDown()
    {
        isFaceDown = true;
        cardBack.gameObject.SetActive(true);
        cardFace.gameObject.SetActive(false);
        cardImage.gameObject.SetActive(false);
    }
}
