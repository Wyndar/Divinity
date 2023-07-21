using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Card;
using static Effect;
using static Game_Manager;
using static Player;

public class CardLogic : MonoBehaviour
{
    public Game_Manager GameManager;

    public EnumConverter enumConverter;

    public Type type;
    public List<PlayType> playTypes;
    public List<Trait> traits;

    public PlayerManager cardOwner, cardController;
    public Transform cardFace, cardBack, cardImage, cardOutline;
    public Sprite image;

    public string id, cardName, cardType, cardText, flavorText;

    public bool isFaceDown, isNormalColour;

    public enum Location
    {
        Deck, HeroDeck, Hand, Field, Grave, Limbo, Outside, Any, Undefined
    }

    public Location currentLocation;
    public int locationOrderNumber;

    public List<CardLogic> targets = new();
    public List<CardLogic> validTargets = new();
    public List<Effect> effects = new();
    public List<BuffHistoryEntry> buffHistoryEntries = new();
    public List<DebuffHistoryEntry> debuffHistoryEntries = new();
    public List<LocationHistoryEntry> locationHistoryEntries = new();
    public List<DamageHistoryEntry> damageHistoryentries = new();

    public int effectCountNumber;
    public int subCountNumber;

    public float movementSpeed = 3f;

    private IEnumerator ActivationCoroutine(int effectNumber, int subEffectNumber)
    {
        GameManager.isActivatingEffect = true;
        GameManager.DisableRayBlocker();

        //these technically don't activate
        if (effects[effectNumber].effectTypes[subEffectNumber] == EffectTypes.WhileDeployed)
        {
            EffectActivationAfterAnimation(effectNumber, subEffectNumber);
            yield break;
        }

        GameManager.StateChange(GameState.EffectActivation);
        float distance = Vector3.Distance(transform.position, cardController.activationZone.position);
        Vector3 originalPosition = transform.position;
        Vector3 direction = (cardController.activationZone.position - transform.position).normalized;
        float distanceTravelled = 0;
        while (distanceTravelled < distance)
        {
            Vector3 translationDistance = (cardController.activationZone.position - transform.position);
            if (translationDistance.magnitude <= direction.magnitude)
                transform.position = cardController.activationZone.position;
            else
                transform.Translate(direction * movementSpeed, Space.World);
            distanceTravelled = Vector3.Distance(originalPosition, transform.position);
            yield return null;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = new(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;

        EffectActivationAfterAnimation(effectNumber, subEffectNumber);
        yield break;
    }

    private IEnumerator ResolutionCoroutine(int effectNumber, int subEffectNumber)
    {
        GameManager.DisableRayBlocker();

        //these technically don't resolve
        if (effects[effectNumber].effectTypes[subEffectNumber] == EffectTypes.WhileDeployed)
        {
            EffectResolutionAfterAnimation(effectNumber, subEffectNumber);
            yield break;
        }

        GameManager.StateChange(GameState.EffectResolution);
        float distance = Vector3.Distance(transform.position, cardController.activationZone.position);
        Vector3 originalPosition = transform.position;
        Vector3 direction = (cardController.activationZone.position - transform.position).normalized;
        float distanceTravelled = 0;
        while (distanceTravelled < distance)
        {
            Vector3 translationDistance = (cardController.activationZone.position - transform.position);
            if (translationDistance.magnitude <= direction.magnitude)
                transform.position = cardController.activationZone.position;
            else
                transform.Translate(direction * movementSpeed, Space.World);
            distanceTravelled = Vector3.Distance(originalPosition, transform.position);
            yield return null;
        }
        Vector3 originalScale = transform.localScale;
        transform.localScale = new(originalScale.x * 2.5f, originalScale.y * 2.5f, originalScale.z * 1f);
        yield return new WaitForSeconds(0.4f);

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        EffectResolutionAfterAnimation(effectNumber, subEffectNumber);
        yield break;
    }

    public List<CardLogic> GetValidTargets(int effectNumber, int subEffectNumber)
    {
        List<CardLogic> allTargetsList = new(FindObjectsOfType<CardLogic>());
        List<CardLogic> returnList = new();
        Effect targetingEffect = effects[effectNumber];
        if (targetingEffect.targetLocation.Count == 0)
            return returnList;

        EffectsUsed effectUsed = targetingEffect.effectsUsed[subEffectNumber];
        Location targetLocation = targetingEffect.targetLocation[subEffectNumber];
        Controller controller = targetingEffect.effectTargetController[subEffectNumber];
        Type type = Type.Undefined;
        PlayType playType = PlayType.Undefined;
        if (targetingEffect.effectTargetType.Count > 0)
            type = targetingEffect.effectTargetType[subEffectNumber];
        if (targetingEffect.effectTargetPlayType.Count > 0)
            playType = targetingEffect.effectTargetPlayType[subEffectNumber];
        foreach(CardLogic target in allTargetsList)
        {
           target.TryGetComponent<CombatantLogic>(out var combatantStats);
            target.TryGetComponent<PlayableLogic>(out var playableStats);
            // basic targeting requirements... don't target wrong location, don't target wrong owner or wrong card types
            if (target == this && targetingEffect.AllowSelfTarget[subEffectNumber] == false)
                continue;
            if (target.currentLocation != targetLocation && targetLocation != Location.Undefined)
                continue;
            if (target.cardController == cardController && controller == Controller.Opponent)
                continue;
            if (target.cardController != cardController && controller == Controller.Player)
                continue;

            //successfully cleaned up the bloody nightmare code by splitting into two seperate types
            if (target.type != type && type != Type.Undefined)
                continue;
            if (!target.playTypes.Contains(playType) && playType != PlayType.Undefined)
                continue;

            // don't target max hp with healing effects
            if (effectUsed == EffectsUsed.Regeneration && (combatantStats == null || combatantStats.maxHp == combatantStats.currentHp))
                continue;
            // don't target no hp with damage effects
            if (effectUsed == EffectsUsed.Damage && (combatantStats == null || combatantStats.currentHp <= 0))
                continue;
            //don't add targets with higher cost when paying for revive or deploy cost
            if ((effectUsed == EffectsUsed.Revive|| effectUsed==EffectsUsed.Deploy) && cardController.costCount < playableStats.cost)
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
            Debug.Log("No valid targets for "+ cardName+"'s "+ effectUsed+ " ability at "+ GameManager.turnPlayer.PlayerName+"'s turn at phase " + GameManager.currentPhase);
        return returnList;
    }

    public void EffectActivation(int countNumber, int subCount)=>
        StartCoroutine(ActivationCoroutine(countNumber, subCount));

    private void EffectActivationAfterAnimation(int countNumber, int subCount)
    {
        Effect activatingEffect = effects[countNumber];
        switch (activatingEffect.effectTypes[subCount])
        {
            //on play
            case EffectTypes.Deployment:
            //in response to
            case EffectTypes.Chain:
            //continuous on field
            case EffectTypes.WhileDeployed:
                TargetCheck(countNumber, subCount);
                break;
            //while on field, manual trigger
            case EffectTypes.Deployed:
            //while in gy manual trigger
            case EffectTypes.Vengeance:
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
        EffectsUsed effectUsed = resolvingEffect.effectsUsed[subCount];
        int effectAmount = resolvingEffect.EffectAmount[subCount];
        effectCountNumber = countNumber;
        subCountNumber = subCount;
        GameManager.currentFocusCardLogic = this;

        switch (effectUsed)
        {
            case EffectsUsed.Reinforce:
                GameManager.DrawCard(effectAmount, cardController);
                break;
            case EffectsUsed.Recruit:
                foreach (CardLogic target in targets)
                    GameManager.SearchCard(target, target.cardController);
                break;
            case EffectsUsed.Recover:
                foreach (CardLogic target in targets)
                    GameManager.RecoverCard(target, cardController);
                break;
            case EffectsUsed.Damage:
                foreach (CardLogic target in targets)
                {
                    target.GetComponent<CombatantLogic>().TakeDamage(effectAmount, false);
                }
                break;
            case EffectsUsed.Regeneration:
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().Heal(effectAmount);
                break;
            case EffectsUsed.Rally:
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().ATKAdjustment(1, true);
                break;
            case EffectsUsed.FreeRevive:
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard("revive", true, cardController);
                    break;
            case EffectsUsed.Revive:
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard("revive", false, cardController);
                break;
            case EffectsUsed.FreeDeploy:
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard("deploy", true, cardController);
                break;
            case EffectsUsed.Deploy:
                foreach (CardLogic target in targets)
                    target.GetComponent<PlayableLogic>().PlayCard("deploy", false, cardController);
                break;
            case EffectsUsed.Vigor:
                foreach (CardLogic target in targets)
                {
                    target.GetComponent<CombatantLogic>().ATKAdjustment(effectAmount, true);
                    target.GetComponent<CombatantLogic>().MaxHPAdjustment(effectAmount, true);
                }
                break;
            case EffectsUsed.Terrify:
                foreach (CardLogic target in targets)
                {
                    target.GetComponent<CombatantLogic>().ATKAdjustment(effectAmount, false);
                    target.GetComponent<CombatantLogic>().MaxHPAdjustment(effectAmount, false);
                }
                break;
            case EffectsUsed.Intimidate:
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().ATKAdjustment(effectAmount, false);
                break;
            case EffectsUsed.Weaken:
                foreach (CardLogic target in targets)
                    target.GetComponent<CombatantLogic>().MaxHPAdjustment(effectAmount, false);
                break;
            case EffectsUsed.Shatter:
                foreach (CardLogic target in targets)
                    target.GetComponent<MonsterLogic>().MonsterDeath();
                break;
            case EffectsUsed.BloodRecovery:
                GameManager.CostChange(effectAmount, cardController, true);
                break;
            case EffectsUsed.Target:
                TargetEffectLogic(countNumber, subCount);
                break;
            case EffectsUsed.Taunt:
                foreach (CardLogic target in targets)
                { target.GetComponent<CombatantLogic>().buffs.targetState = Buff.TargetState.Default; }
                break;
            case EffectsUsed.Stealth:
                foreach (CardLogic target in targets)
                {
                    target.GetComponent<CombatantLogic>().buffs.targetState=Buff.TargetState.Stealth; }
                break;
            case EffectsUsed.Armor:
                foreach (CardLogic target in targets)
                {
                    target.GetComponent<CombatantLogic>().armor = effectAmount;
                    GameObject icon = target.cardController.armorIcons[locationOrderNumber];
                    icon.SetActive(true);
                    icon.GetComponentInChildren<TMP_Text>().text = effectAmount.ToString();

                }
                break;
        }
        if (!GameManager.isWaitingForResponse)
            FinishResolution(countNumber, subCount);
    }

    public void FinishResolution(int countNumber, int subCount)
    {
        Effect resolvingEffect = this.effects[countNumber];
        GameManager.isActivatingEffect = false;
        if (resolvingEffect.currentActivations < resolvingEffect.maxActivations)
            resolvingEffect.currentActivations++;

        //chainlist for effect triggers
        GameManager.GetEffectTriggers(countNumber, subCount, this);

        CheckSubsequentEffects(countNumber, subCount);
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
        if (resolvingEffect.effectsUsed.Count <= subCount + 1)
            return true;
        if (resolvingEffect.effectTypes[subCount] != resolvingEffect.effectTypes[subCount + 1])
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
        Effect targetingEffect = effects[countNumber];
        //auto self target effects
        if (targetingEffect.AllowSelfTarget[subCount] == true && targetingEffect.EffectTargetAmount[subCount] == 1)
        {
            targets = new();
            targets.Add(this);
        }
        else
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
        foreach (Effect effect in effects)
            effect.currentActivations = 0;
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
