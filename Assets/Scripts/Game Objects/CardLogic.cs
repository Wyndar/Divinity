using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class CardLogic : MonoBehaviour
{
    public Game_Manager GameManager;

    public EnumConverter enumConverter;

    public PlayerManager cardOwner, cardController;
    public Transform cardFace, cardBack, cardImage, cardOutline;
    public Sprite image;

    public string id, cardName, cardType, cardText, flavorText;

    public bool isFaceDown, isNormalColour;

    public enum Location
    {
        Deck,HeroDeck,Hand,Field,Grave,Limbo,Outside,Undefined
    }

    public Location currentLocation;
    public int locationOrderNumber;

    public List<CardLogic> targets = new();
    public List<CardLogic> validTargets = new();
    public List<Effect> effects;
    public List<int> currentActivations = new();
    public int effectCountNumber;
    public int subCountNumber;

    private IEnumerator ActivationCoroutine(int effectNumber, int subEffectNumber)
    {
        GameManager.isActivatingEffect = true;
GameManager.StateChange(Game_Manager.GameState.EffectActivation);
        GameManager.DisableRayBlocker();
        Vector3 originalPosition = transform.localPosition;
        transform.position = cardController.activationZone.position;
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
        transform.position = cardController.activationZone.position;
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
        Effect targetingEffect = effects[effectNumber];
        if (targetingEffect.TargetLocation == null)
            return returnList;

        string effectUsed = targetingEffect.EffectUsed[subEffectNumber];
        string targetLocation = targetingEffect.TargetLocation[subEffectNumber];
        if (targetLocation == "none")
            return returnList;
        string controller = targetingEffect.EffectTargetOwner[subEffectNumber];
        string targetType = targetingEffect.EffectTargetType[subEffectNumber];
        for (int i = 0; i < allTargetsList.Count; i++)
        {
            CardLogic target = allTargetsList[i];
            allTargetsList[i].TryGetComponent<CombatantLogic>(out var combatantStats);
            allTargetsList[i].TryGetComponent<PlayableLogic>(out var playableStats);
            // basic targeting requirements... don't target wrong location, don't target wrong owner or wrong card types
            if (target == this && targetingEffect.AllowSelfTarget[subEffectNumber] == false)
                continue;
            if (target.currentLocation != enumConverter.LocationStringToEnum(targetLocation) && targetLocation != "any")
                continue;
            if (target.cardController == cardController && controller == "Opponent")
                continue;
            if (target.cardController != cardController && controller == "Player")
                continue;

            //I know it looks funny and wasteful but believe me when I say this is the best way to write these next few lines, I tried everything
            if (target.cardType != targetType && targetType != "any" && targetType != "playable" && targetType != "combatant")
                continue;
            if (targetType == "combatant" && target.cardType == "spell")
                continue;
            if (targetType == "playable" && target.cardType == "god")
                continue;
            //do NOT edit this nightmare, do NOT attempt to optimize...EVER!

            // don't target max hp with healing effects
            if (effectUsed == "Regeneration" && (combatantStats == null || combatantStats.maxHp == combatantStats.currentHp))
                continue;
            // don't target no hp with damage effects
            if (effectUsed == "Damage" && (combatantStats == null || combatantStats.currentHp <= 0))
                continue;
            //don't add targets with higher cost when paying for revive or deploy cost
            if ((effectUsed == "Revive"|| effectUsed=="Deploy") && cardController.costCount < playableStats.cost)
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
            Debug.Log("No valid targets for "+ cardName+"'s "+ effectUsed+ " effect at "+ GameManager.turnPlayer.PlayerName+"'s turn at phase " + GameManager.currentPhase);
        return returnList;
    }

    public void EffectActivation(int countNumber, int subCount)=>
        StartCoroutine(ActivationCoroutine(countNumber, subCount));

    private void EffectActivationAfterAnimation(int countNumber, int subCount)
    {
        Effect activatingEffect = effects[countNumber];
        switch (activatingEffect.EffectType[subCount])
        {
            case "Deployment":
            case "Chain":
                TargetCheck(countNumber, subCount);
                break;
            case "Deployed":
                if (currentLocation == enumConverter.LocationStringToEnum(activatingEffect.ActivationLocation))
                    TargetCheck(countNumber, subCount);
                break;
        }
        GameManager.currentFocusCardLogic = this;
    }

    public void EffectResolution(int countNumber, int subCount) =>StartCoroutine(ResolutionCoroutine(countNumber, subCount));

    private void EffectResolutionAfterAnimation(int countNumber, int subCount)
    {
        Effect resolvingEffect = effects[countNumber];
        string effectUsed = resolvingEffect.EffectUsed[subCount];
        int effectAmount = resolvingEffect.EffectAmount[subCount];

        switch (effectUsed)
        {
            case "Reinforce":
                GameManager.DrawCard(effectAmount, cardController);
                break;
            case "Recruit":
                foreach (CardLogic target in targets)
                    GameManager.SearchCard(target, target.cardController);
                break;
            case "Recover":
                foreach (CardLogic target in targets)
                    GameManager.RecoverCard(target, cardController);
                break;
            case "Damage":
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().TakeDamage(effectAmount, false);
                break;
            case "Regeneration":
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().Heal(effectAmount);
                break;
            case "Rally":
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().ATKAdjustment(1, true);
                break;
            case "Free Revive":
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard("revive", true, cardController);
                    break;
            case "Revive":
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard("revive", false, cardController);
                break;
            case "Free Deploy":
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard("deploy", true, cardController);
                break;
            case "Deploy":
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard("deploy", false, cardController);
                break;
            case "Vigor":
                foreach (CardLogic target in targets)
                {
                    target.GetComponent<CombatantLogic>().ATKAdjustment(effectAmount, true);
                    target.GetComponent<CombatantLogic>().MaxHPAdjustment(effectAmount, true);
                }
                break;
            case "Terrify":
                foreach (CardLogic target in targets)
                {
                    target.GetComponent<CombatantLogic>().ATKAdjustment(effectAmount, false);
                    target.GetComponent<CombatantLogic>().MaxHPAdjustment(effectAmount, false);
                }
                break;
            case "Intimidate":
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().ATKAdjustment(effectAmount, false);
                break;
            case "Weaken":
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().MaxHPAdjustment(effectAmount, false);
                break;
            case "Shatter":
                foreach (CardLogic target in targets)
                    target.GetComponent<MonsterLogic>().MonsterDeath();
                break;
            case "Blood Recovery":
                GameManager.CostChange(effectAmount, cardController, true);
                break;
            case "Target":
                TargetEffectLogic(countNumber, subCount);
                break;

        }
        GameManager.isActivatingEffect = false;
        if (resolvingEffect.MaxActivations != 0 && currentActivations[countNumber] != 0)
            currentActivations[countNumber] -= 1;

        //chainlist for effect triggers
        GameManager.GetEffectTriggers(countNumber, subCount, this);

        CheckSubsequentEffects(countNumber,subCount);
    }

    private void CheckSubsequentEffects(int countNumber, int subCount)
    {
        if (ResolveSubsequentSubeffects(countNumber, subCount))
        {
            GameManager.ClearEffectTargetImages();
            if (targets != null)
                targets.Clear();
            if (validTargets != null)
                validTargets.Clear();
            if (cardType == "spell")
                gameObject.GetComponent<PlayableLogic>().MoveToGrave();

            //resolve chain after all possible effect chains are linked
            if (cardController.isAI)
                cardController.AIManager.isPerformingAction = false;
            GameManager.ChainResolution();
        };
    }

    private bool ResolveSubsequentSubeffects(int countNumber, int subCount)
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
            GameManager.currentFocusCardLogic = this;
            if (cardController.isAI)
                OptionalEffectResolution(cardController.AIManager.ActivateOptionalEffect());
            else
                GameManager.EnableActivationPanel();
            return false;
        }
        //dependent on targets of previous effect
        if (resolvingEffect.EffectTargetAmount[subCount + 1] == 98)
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

    private void TargetEffectLogic(int countNumber, int subCount)
    {
        Effect resolvingEffect = effects[countNumber];
        int[] effectAmountIndexesToChange = resolvingEffect.EffectAmount.FindAllIndexof(98);
        targets[0].TryGetComponent<CombatantLogic>(out var combatantStats);
        targets[0].TryGetComponent<PlayableLogic>(out var playableStats);

        foreach (int index in effectAmountIndexesToChange)
        {
            Debug.Log(index);
            if (resolvingEffect.TargetCountModifier != null && resolvingEffect.TargetCountModifier[subCount]!=0)
            {
                resolvingEffect.EffectAmount[index] = Mathf.CeilToInt(targets.Count * resolvingEffect.TargetCountModifier[subCount]);
                Debug.Log(resolvingEffect.EffectAmount[index]);
                continue;
            }

            string checkedStat = resolvingEffect.TargetStat[subCount];
            int amount = resolvingEffect.EffectAmount[index];
            switch (checkedStat)
            {
                case "current atk":
                    amount = combatantStats.currentAtk;
                    break;
                case "cost":
                    amount = playableStats.cost;
                    break;
            }
        }
    }

    private void TargetCheck(int countNumber, int subCount)
    {
        Effect targetingEffect = effects[countNumber];
        if (targetingEffect.EffectTargetAmount == null)
        {
            EffectResolution(countNumber, subCount);
            return;
        }
        if (targetingEffect.EffectTargetAmount[subCount] == 0)
        {
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
                    EffectResolution(countNumber, subCount);
                    return;
                }
                effectCountNumber = countNumber;
                subCountNumber = subCount;
                GameManager.currentFocusCardLogic = this;
                foreach(CardLogic target in validTargets)
                {
                    if (target.cardType == "monster" && target.currentLocation == Location.Field)
                        target.cardController.effectTargets[target.locationOrderNumber].SetActive(true);
                    if (target.cardType == "god" && target.currentLocation == Location.Field)
                        target.cardController.heroEffectTarget.SetActive(true);
                    if (target.currentLocation == Location.Grave)
                        target.cardController.graveTarget.SetActive(true);
                    if (target.currentLocation == Location.Deck)
                        target.cardController.deckTarget.SetActive(true);
                }
                if(cardController.isAI)
                {
                    cardController.AIManager.GetEffectTarget();
                    return;
                }
                if (targetingEffect.TargetLocation[subCount] != "Field")
                    GameManager.EnableCardScrollScreen(validTargets, !targetingEffect.EffectActivationIsMandatory[subCount]);
                return;
            }
            if (targetingEffect.TargetingType[subCount] == "auto")
            {
                AutoTargetAcquisition(countNumber, subCount);
                return;
            }
            if (targetingEffect.TargetingType[subCount] == "random")
            {
                RandomTargetAcquisition(countNumber, subCount);
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
            EffectResolution(countNumber, subCount);
            return;
        }
    }

    //called by ux manager clicked GameObject method with current focus card logic count and subcount
    public void ManualTargetAcquisition(int countNumber, int subCount)
    {
        if (GameManager.gameState != Game_Manager.GameState.Targeting)
            return;

        CardLogic targeter = GameManager.currentFocusCardLogic;
        if (!targeter.validTargets.Contains(this))
            return;
        if (targeter.targets == null)
            targeter.targets = new();
        else if (targeter.targets.Contains(this))
            return;
        targeter.targets.Add(this);
        //if you hit the needed amount of targets or all valid targets are taken, resolve
        if (targeter.targets.Count == targeter.effects[countNumber].EffectTargetAmount[subCount] || targeter.targets.Count == targeter.validTargets.Count)
        {
            GameManager.DisableCardScrollScreen();
            targeter.EffectResolution(countNumber, subCount);
            return;
        }
    }

    public void ManualTargetRemoval(int countNumber, int subCount)
    {
        if (GameManager.gameState != Game_Manager.GameState.Targeting)
            return;
        CardLogic targeter = GameManager.currentFocusCardLogic;
        if (targeter.targets == null)
            return;
        if (targeter.targets.Contains(this))
            targeter.targets.Remove(this);
    }

    //less auto more all target
    public void AutoTargetAcquisition(int countNumber, int subCount)
    {
        targets = new(validTargets);
        EffectResolution(countNumber, subCount);
        return;
    }

    public void RandomTargetAcquisition(int countNumber, int subCount)
    {
        int targetsLeft = effects[countNumber].EffectTargetAmount[subCount];
        while (targetsLeft > 0 && validTargets.Count>targets.Count)
        {
            int randomNumber = Random.Range(0, validTargets.Count);
            if (targets.Contains(validTargets[randomNumber]))
                continue;
            targets.Add(validTargets[randomNumber]);
            targetsLeft--;
        }
        EffectResolution(countNumber, subCount);
        return;
    }

    public void OptionalEffectResolution(bool used)
    {
        if(!used)
        {
            CheckSubsequentEffects(effectCountNumber, subCountNumber);
            return;
        }
        //if you need the targets from previous effect to resolve
        if (effects[effectCountNumber].TargetingType != null && effects[effectCountNumber].EffectTargetAmount[subCountNumber] == 98)
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

    public void GreyScaleEffect()
    {
        isNormalColour = false;
        cardFace.GetComponent<SpriteRenderer>().color = Color.grey;
        cardImage.GetComponent<SpriteRenderer>().color = Color.grey;
    }

    public void NormalColour()
    {
        isNormalColour = true;
        cardFace.GetComponent<SpriteRenderer>().color = Color.white;
        cardImage.GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void ControllerSwap(PlayerManager player)
    {
        cardController = player;
        gameObject.transform.rotation = player.deck.transform.rotation;
    }
}
