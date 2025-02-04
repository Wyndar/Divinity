using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public class GameBattleManager : GameManager
{
    [SerializeField]
    private SaveManager SaveManager;
    [SerializeField]
    private TurnManager TurnManager;
    [SerializeField]
    private SecondaryUIManager UIManager;
    [SerializeField]
    private PrimaryUIManager MainUIManager;
    [SerializeField]
    private DeckLoaderManager DeckManager;
    [SerializeField]
    private StatusManager StatusManager;
    [SerializeField]
    private ToolTipManager ToolTipManager;

    public PlayerManager BluePlayerManager, RedPlayerManager, turnPlayer, turnOpponent;

    public bool isNotFirstDraw, isPlayingCard, isActivatingEffect, isChecking, isWaitingForResponse, isShowingInfo, isShowingLog;

    public GameState gameState;
    public Phase currentPhase;
    public List<GameLogHistoryEntry> gameLogHistoryEntries = new();
    public List<CardLogic> activationChainList = new();
    public List<SubEffect> activationChainSubEffectList = new();
    public CardSlot[] Row1, Row2, Row3, Row4;

    public CardLogic currentFocusCardLogic;
    public CardSlot currentFocusCardSlot;
    public GameObject phaseChangeButton, popUpPanel;

    public TMP_Text bluePlayerText, redPlayerText, turnCountText, turnPhaseText, popUpPanelText, phaseChangeButtonText;

    public int turnCount;
    public string BluePlayerPath, RedPlayerPath;

    public float loadStartTime;
    public float loadEndTime;

    public event Action<Phase> OnPhaseChange;
    public event Action<SubEffect, CardLogic> OnEffectTrigger;
    public event Action<GameState, CardLogic> OnStateChange;

    private Queue<(CardLogic, SubEffect)> effectQueue = new Queue<(CardLogic, SubEffect)>();
    private bool isProcessingEffects = false;
    private void Start()
    {
        InitializeGame();
        InitializePlayers();
        InitializeBoard();
        InitializeUI();
        DebugLoadTime();
        StartCoroutine(TurnManager.ChooseFirstPlayer());
    }

    private void InitializeGame()
    {
        loadStartTime = Time.realtimeSinceStartup;
        AudioManager.FindBattleOBJ();
        AudioManager.LoadSFX();
        LoadToolTips();
    }

    private void InitializeUI()
    {
        popUpPanelText = popUpPanel.GetComponentInChildren<TMP_Text>();
        phaseChangeButtonText = phaseChangeButton.GetComponentInChildren<TMP_Text>();
        UIManager.UIUpdate(BluePlayerManager);
        UIManager.UIUpdate(RedPlayerManager);
    }

    private void InitializePlayers()
    {
        LoadPlayers(BluePlayerManager, BluePlayerPath);
        LoadPlayers(RedPlayerManager, RedPlayerPath);
        BluePlayerManager.SetShield(0, BluePlayerManager.shieldCount);
        RedPlayerManager.SetShield(0, RedPlayerManager.shieldCount);
    }

    private void InitializeBoard()
    {
        RedPlayerManager.cardSlots = new();
        BluePlayerManager.cardSlots = new();
        InitializeSlots(Row1, RedPlayerManager, true);
        InitializeSlots(Row2, RedPlayerManager, false);
        InitializeSlots(Row3, BluePlayerManager, false);
        InitializeSlots(Row4, BluePlayerManager, true);
        for (int i = 0; i < BluePlayerManager.hand.transform.childCount; i++)
            BluePlayerManager.handSlots.Add(BluePlayerManager.hand.transform.GetChild(i).GetComponent<HandSlot>());
        for (int i = 0; i < RedPlayerManager.hand.transform.childCount; i++)
            RedPlayerManager.handSlots.Add(RedPlayerManager.hand.transform.GetChild(i).GetComponent<HandSlot>());
    }
    private void InitializeSlots(CardSlot[] row, PlayerManager player, bool isFrontline)
    {
        foreach (CardSlot slot in row)
        {
            slot.InitializeSlot(this);
            slot.ChangeController(player);
            slot.isFrontline = isFrontline;
        }
        player.cardSlots.AddRange(row);
    }
    private void DebugLoadTime()
    {
        loadEndTime = Time.realtimeSinceStartup;
        Debug.Log($"Card Load time is : {loadEndTime - loadStartTime} seconds");
    }

    //will remove this later
    public void LoadPlayers(PlayerManager playerManager, string path)
    {
        Player player = SaveManager.LoadPlayerFromJson(path);

        playerManager.PlayerName = player.PlayerName;
        playerManager.PlayerID = player.PlayerID;
        playerManager.DeckPath = player.DeckPath;
        playerManager.HeroDeckPath = player.HeroDeckPath;
        playerManager.isAI = player.IsAI;
        playerManager.isLocal = player.IsLocal;
        if (playerManager.isAI)
            playerManager.transform.GetChild(0).gameObject.SetActive(true);
        LoadDeckID(playerManager);
    }

    public void LoadDeckID(PlayerManager playerManager)
    {
        playerManager.deckListID.AddRange(SaveManager.LoadIDFromJson(playerManager.DeckPath));
        playerManager.heroDeckListID.AddRange(SaveManager.LoadIDFromJson(playerManager.HeroDeckPath));
        playerManager.deckLogicList.AddRange(DeckManager.LoadDeck(playerManager.deckListID, playerManager.deckList,
            playerManager.deck, playerManager, false));
        playerManager.heroDeckLogicList.AddRange(DeckManager.LoadDeck(playerManager.heroDeckListID,
            playerManager.heroDeckList, playerManager.shield, playerManager, true));
    }

    public void LoadToolTips() => ToolTipManager.tooltipInfos.AddRange(SaveManager.LoadToolTipInfoDatabase());
    public void ShuffleHand(PlayerManager player)
    {
        AudioManager.NewAudioPrefab(AudioManager.shuffleHand);
        player.handLogicList = player.handLogicList.OrderBy(x => UnityEngine.Random.value).ToList();
        ArrangeHand(player);
    }

    private static void ArrangeHand(PlayerManager player)
    {
        float xDist = 2.1f;
        float yDist = -0.82f;
        float cardSize = 0.511f;
        int topCount = Mathf.FloorToInt(player.handSize / 2 + 0.5f);
        int bottomCount = player.handSize - topCount;

        for (int i = 0; i < player.handSlots.Count; i++)
        {
            HandSlot slot = player.handSlots[i];
            if (slot.cardInZone == null) continue;

            slot.transform.localPosition = CalculateCardPosition(i, player.handSize, topCount, bottomCount, xDist, yDist, cardSize);
        }
    }

    private static Vector3 CalculateCardPosition(int index, int handSize, int topCount, int bottomCount, float xDist, float yDist, float cardSize)
    {
        int trueCount = index - topCount + 1;
        float yPos = handSize < 4 ? yDist / 2 : index < topCount ? 0 : yDist;
        float xPos = handSize < 4 ? xDist / (handSize + 1) * (index + 1) :
                      (trueCount <= 0 ? cardSize * index : cardSize * (trueCount - 1));

        if ((trueCount <= 0 && topCount == 4) || (trueCount > 0 && bottomCount == 4))
            xPos += 0.11f * (trueCount <= 0 ? index + 1 : trueCount);

        return new Vector3(xPos, yPos, 0);
    }

    public void StateReset()
    {
        StateChange(GameState.Open);
        if (currentFocusCardLogic != null)
            currentFocusCardLogic.RemoveFocusCardLogic();
        if (currentFocusCardSlot != null)
            currentFocusCardSlot.DeselectSlot();
        if (currentPhase == Phase.MainPhase)
            ShowValidMoves(turnPlayer);
        if (currentPhase == Phase.BattlePhase)
            ShowValidAttackers(turnPlayer);
        if (turnPlayer.isAI)
            turnPlayer.AIManager.MakeDecision();
    }

    public void StateChange(GameState state)
    {
        UIManager.UIUpdate(BluePlayerManager);
        UIManager.UIUpdate(RedPlayerManager);
        gameState = state;
        switch (gameState)
        {
            case GameState.Open:
            case GameState.EffectActivation:
            case GameState.Targeting:
            case GameState.ChainResolution:
            case GameState.Moving:
                return;
        }
        OnStateChange?.Invoke(gameState, currentFocusCardLogic);
    }

    public void PhaseChange(Phase phase)
    {
        currentPhase = phase;
        OnPhaseChange?.Invoke(phase);
        ChainResolution();
    }

    public void InvokeEffectTrigger(SubEffect subEffect, CardLogic cardLogic) => OnEffectTrigger?.Invoke(subEffect, cardLogic);
    public void ChainResolution()
    {
        //nothing to do if in middle of effect
        if (isActivatingEffect || isWaitingForResponse || isPlayingCard)
            return;
        //if empty chain, reset and get new decision
        if (activationChainList.Count == 0)
        {
            StateReset();
            return;
        }
        //else, resolve chain
        ChainResolver();
    }

    private void ChainResolver()
    {
        StateChange(GameState.ChainResolution);
        //get reference to each so that they can be safely removed then activated before coroutines ruin call sequence
        CardLogic cardLogic = activationChainList[0];
        SubEffect subEffect = activationChainSubEffectList[0];
        activationChainList.RemoveAt(0);
        activationChainSubEffectList.RemoveAt(0);
        //for non ai players to decide to use optionals
        if (!subEffect.EffectActivationIsMandatory && !cardLogic.dataLogic.cardController.isAI)
        {
            cardLogic.SetFocusCardLogic();
            cardLogic.effectLogic.focusSubEffect = subEffect;
            EnableActivationPanel();
            return;
        }
        //ai optionals negation check
        if (!subEffect.EffectActivationIsMandatory && cardLogic.dataLogic.cardController.isAI)
            if (!cardLogic.dataLogic.cardController.AIManager.ActivateOptionalEffect())
                ChainResolution();
        //else it's mandatory or has been accepted to go forward
        cardLogic.effectLogic.EffectActivation(subEffect);
    }

    public void DisableRayBlocker() => MainUIManager.DisableRayBlocker();

    public void ShowEffectInfoPanel() => MainUIManager.ShowEffectInfoPanel();

    public void DisableEffectInfoPanels() => MainUIManager.DisableEffectInfoPanels();

    public void EnableActivationPanel() => MainUIManager.EnableEffectActivationPanel();

    public void ShowShieldPrompt(PlayerManager cardOwner) => MainUIManager.ShowShieldPrompt(cardOwner);

    public void StatusCountdown(CardStatus cardStatus) => StatusManager.Countdown(cardStatus);
    public void StatusCountdownReset(CardStatus cardStatus) => StatusManager.CountdownReset(cardStatus);

    public void AllEffectsRefresh(PlayerManager player)
    {
        foreach (CardLogic cardLogic in player.handLogicList)
            cardLogic.effectLogic.EffectRefresh();
        foreach (CardLogic cardLogic in player.fieldLogicList)
        {
            cardLogic.effectLogic.EffectRefresh();
            if (cardLogic.dataLogic.type == Type.Fighter)
                cardLogic.GetComponent<MonsterLogic>().hasMoved = false;
        }
        foreach (CardLogic cardLogic in player.graveLogicList)
            cardLogic.effectLogic.EffectRefresh();
        player.heroCardLogic.effectLogic.EffectRefresh();
    }

    public void AllAttacksRefresh(PlayerManager player)
    {
        foreach (CardLogic cardLogic in player.fieldLogicList)
            cardLogic.GetComponent<CombatantLogic>().AttackRefresh();
        player.heroCardLogic.GetComponent<CombatantLogic>().AttackRefresh();
    }


    public void AllTimersCountdown()
    {
        PlayerTimerCountDown(turnPlayer);
        PlayerTimerCountDown(turnOpponent);
    }

    //to avoid  changed list errors
    public void PlayerTimerCountDown(PlayerManager player)
    {
        foreach (CardLogic cardLogic in player.fieldLogicList)
        {
            CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
            if (combatantLogic.hasDoneCountdown)
                continue;
            else
            {
                combatantLogic.TurnTimer();
                break;
            }
        }
        CombatantLogic heroCombatantLogic = player.heroCardLogic.GetComponent<CombatantLogic>();
        if (!heroCombatantLogic.hasDoneCountdown)
            heroCombatantLogic.TurnTimer();
    }

    public void AllCountdownReset()
    {
        PlayerCountDownReset(turnPlayer);
        PlayerCountDownReset(turnOpponent);
    }

    public void PlayerCountDownReset(PlayerManager player)
    {
        foreach (CardLogic cardLogic in player.fieldLogicList)
            cardLogic.GetComponent<CombatantLogic>().ResetCountdown();
        player.heroCardLogic.GetComponent<CombatantLogic>().ResetCountdown();
    }

    public void ShieldRefresh(PlayerManager player) => player.heroCardLogic.ShieldRefresh();

    public void SwitchControl(PlayerManager player)
    {
        if (player.isAI || !player.isLocal || player.enemy.isAI || !player.enemy.isLocal)
            return;
        foreach (CardLogic cardLogic in player.enemy.handLogicList)
            cardLogic.visualsLogic.Flip(true);
        foreach (CardLogic cardLogic in player.handLogicList)
            cardLogic.visualsLogic.Flip(false);
    }

    public void ShowValidMoves(PlayerManager player)
    {
        player.playableLogicList.Clear();
        player.canUseEffectLogicList.Clear();
        player.canUseSubEffectList.Clear();
        foreach (CardLogic cardLogic in player.handLogicList)
        {
            bool legal = cardLogic.GetComponent<PlayableLogic>().LegalPlayCheck(false, player) == null;
            cardLogic.visualsLogic.GreyScaleEffect(!legal);
            if (legal)
                player.playableLogicList.Add(cardLogic);
        }
        foreach (CardLogic cardLogic in player.fieldLogicList)
        {
            if (cardLogic.GetComponent<CombatantLogic>().ImmobilityCheck())
                continue;
            bool shouldAdd = false;
            foreach (Effect effect in cardLogic.effectLogic.effects)
            {
                if (effect.currentActivations >= effect.maxActivations && effect.maxActivations != 0)
                    continue;
                foreach (SubEffect subEffect in effect.SubEffects)
                {
                    if (subEffect.effectType != EffectTypes.Deployed)
                        continue;
                    if (subEffect.effectUsed != EffectsUsed.BloodCost
                        && cardLogic.targetingLogic.GetValidTargets(subEffect, false).Count == 0)
                    {
                        if (!subEffect.EffectActivationIsMandatory) continue;
                        shouldAdd = false;
                        break;
                    }
                    if (subEffect.effectUsed == EffectsUsed.BloodCost &&
                        player.BloodAttunementCheck(Enum.Parse<Attunement>(subEffect.TargetStats[0])) <= subEffect.effectAmount)
                    {
                        if (!subEffect.EffectActivationIsMandatory) continue;
                        shouldAdd = false;
                        break;
                    }
                    shouldAdd = true;
                }
                if (!shouldAdd)
                    continue;
                player.canUseEffectLogicList.Add(cardLogic);
                player.canUseSubEffectList.Add(effect.SubEffects[0]);
            }
        }
        foreach (Effect effect in player.heroCardLogic.effectLogic.effects)
        {
            if (effect.currentActivations >= effect.maxActivations && effect.maxActivations != 0)
                continue;
            bool shouldAdd = false;
            foreach (SubEffect subEffect in effect.SubEffects)
            {
                if (subEffect.effectType != EffectTypes.Deployed)
                    continue;
                if (subEffect.effectUsed != EffectsUsed.BloodCost
                    && player.heroCardLogic.targetingLogic.GetValidTargets(subEffect, false).Count == 0)
                {
                    if (!subEffect.EffectActivationIsMandatory) continue;
                    shouldAdd = false;
                    break;
                }
                if (subEffect.effectUsed == EffectsUsed.BloodCost &&
                    player.BloodAttunementCheck(Enum.Parse<Attunement>(subEffect.TargetStats[0])) <= subEffect.effectAmount)
                {
                    if (!subEffect.EffectActivationIsMandatory) continue;
                    shouldAdd = false;
                    break;
                }
                shouldAdd = true;
            }
            if (!shouldAdd)
                continue;
            player.canUseEffectLogicList.Add(player.heroCardLogic);
            player.canUseSubEffectList.Add(effect.SubEffects[0]);
        }
    }

    public void ShowValidAttackers(PlayerManager player)
    {
        player.canAttackLogicList.Clear();
        foreach (CardLogic cardLogic in player.fieldLogicList)
        {
            CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
            if (combatantLogic.ValidAttackerCheck())
                player.canAttackLogicList.Add(cardLogic);
        }
        //implement atk for god???
    }

    public void ClearEffectTargetImages()
    {
        foreach (CardSlot slot in Row1)
            slot.effectTarget.SetActive(false);
        foreach (CardSlot slot in Row2)
            slot.effectTarget.SetActive(false);
        foreach (CardSlot slot in Row3)
            slot.effectTarget.SetActive(false);
        foreach (CardSlot slot in Row4)
            slot.effectTarget.SetActive(false);

        RedPlayerManager.heroEffectTarget.SetActive(false);
        RedPlayerManager.deckTarget.SetActive(false);
        RedPlayerManager.graveTarget.SetActive(false);
        BluePlayerManager.heroEffectTarget.SetActive(false);
        BluePlayerManager.deckTarget.SetActive(false);
        BluePlayerManager.graveTarget.SetActive(false);
    }

    public void GameOver(PlayerManager winner)
    {
        if (RedPlayerManager.isAI)
            RedPlayerManager.AIManager.StopAllCoroutines();
        if (BluePlayerManager.isAI)
            BluePlayerManager.AIManager.StopAllCoroutines();
        StopAllCoroutines();
        AudioManager.GameOverSequence(winner == BluePlayerManager);
        MainUIManager.GameOver(winner);
        isNotFirstDraw = false;

    }

    public void ClearAttackTargetImages()
    {
        foreach (CardSlot slot in Row1)
            slot.attackTarget.SetActive(false);
        foreach (CardSlot slot in Row2)
            slot.attackTarget.SetActive(false);
        foreach (CardSlot slot in Row3)
            slot.attackTarget.SetActive(false);
        foreach (CardSlot slot in Row4)
            slot.attackTarget.SetActive(false);

        RedPlayerManager.heroAttackTarget.SetActive(false);
        BluePlayerManager.heroAttackTarget.SetActive(false);
    }

    public void LogToScroll()
    {
        List<CardLogic> loggedLogics = new();
        foreach (GameLogHistoryEntry entry in gameLogHistoryEntries)
            loggedLogics.Add(entry.loggedCard);
    }

    public void DisableLog() => MainUIManager.DisableLog();

    public void EnableCardScrollScreen(List<CardLogic> cardLogics, bool shouldShowButton) =>
        MainUIManager.EnableCardScrollScreen(cardLogics, shouldShowButton);

    public void DisableCardScrollScreen() => MainUIManager.DisableCardScrollScreen();

    public void ForceBloodgain(string attunement)
    {
        BluePlayerManager.BloodGain(Enum.Parse<Attunement>(attunement), 1);
        StateReset();
    }
    public void ForceEnemnyBloodgain(string attunement)
    {
        RedPlayerManager.BloodGain(Enum.Parse<Attunement>(attunement), 1);
        StateReset();
    }
    // Helper Method to Add Cards to Hand using Coroutines
    private IEnumerator AddCardToHand(CardLogic card, PlayerManager player)
    {
        foreach (HandSlot handSlot in player.handSlots)
        {
            if (handSlot.cardInZone != null) continue;
            if (player.handSize >= 10) yield break;

            card.gameObject.SetActive(true);
            card.LocationChange(Location.Hand, player.handSize);
            card.transform.SetParent(handSlot.transform, false);
            card.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            card.visualsLogic.Flip(!player.isLocal || player.isAI || player != turnPlayer && !player.enemy.isAI && player.enemy.isLocal);

            AudioSource drawSound = AudioManager.NewAudioPrefab(AudioManager.draw);
            yield return new WaitUntil(() => drawSound == null || !drawSound.isPlaying);

            handSlot.cardInZone = card;
            player.handLogicList.Add(card);
            player.handSize = player.handLogicList.Count;
            RegisterCardEvents(card);
            yield break;
        }
    }

    private void RegisterCardEvents(CardLogic card)
    {
        OnPhaseChange += card.GetPhaseTriggers;
        OnEffectTrigger += card.GetEffectTriggers;
        OnStateChange += card.GetStateTriggers;
    }

    public IEnumerator RandomCardDraw(List<CardLogic> sourceDeck, int drawAmount, PlayerManager player)
    {
        if (player.handSize >= 10 || sourceDeck.Count == 0) yield break;
        bool drewCards = false;

        while (drawAmount > 0 && sourceDeck.Count > 0)
        {
            CardLogic randomCard = sourceDeck[UnityEngine.Random.Range(0, sourceDeck.Count)];
            yield return StartCoroutine(AddCardToHand(randomCard, player));
            sourceDeck.Remove(randomCard);
            drawAmount--;
            drewCards = true;
        }

        FinalizeCardDraw(player, drewCards);
    }

    public IEnumerator SearchCard(CardLogic card, CardLogic activatingCard)
    {
        if (activatingCard.dataLogic.cardOwner.handSize >= 10) yield break;
        yield return StartCoroutine(AddCardToHand(card, activatingCard.dataLogic.cardOwner));
        card.dataLogic.cardController.deckLogicList.Remove(card);
        FinalizeCardDraw(activatingCard.dataLogic.cardOwner, true);
        yield break;
    }

    public IEnumerator RecoverCard(CardLogic card)
    {
        PlayerManager player = card.dataLogic.cardOwner;
        if (player.handSize >= 10) yield break;
        yield return StartCoroutine(AddCardToHand(card, player));
        player.graveLogicList.Remove(card);
        FinalizeCardDraw(player, true);
        yield break;
    }

    private void FinalizeCardDraw(PlayerManager player, bool drewCards)
    {
        if (drewCards)
        {
            ShuffleHand(player);
            if (isNotFirstDraw)
                StateChange(GameState.Reinforcement);
        }
    }

    public void QueueEffect(CardLogic caster, SubEffect subEffect)
    {
        effectQueue.Enqueue((caster, subEffect));
        if (!isProcessingEffects)
        {
            StartCoroutine(ProcessEffectQueue());
        }
    }

    private IEnumerator ProcessEffectQueue()
    {
        isProcessingEffects = true;
        while (effectQueue.Count > 0)
        {
            var (caster, subEffect) = effectQueue.Dequeue();
            ExecuteEffect(caster, subEffect);
            yield return new WaitForSeconds(0.1f); //delay for frame freeze avoidance, can add animation here
        }
        isProcessingEffects = false;
    }

    private void ExecuteEffect(CardLogic caster, SubEffect subEffect)
    {
        //IEffectStrategy effectStrategy = EffectFactory.CreateEffect(subEffect);
        //effectStrategy.Execute(subEffect, caster, caster.gameManager.currentFocusCardLogic);
    }
}