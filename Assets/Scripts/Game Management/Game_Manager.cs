using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Linq;

public class Game_Manager : MonoBehaviour
{
    [SerializeField]
    private SaveManager SaveManager;
    [SerializeField]
    private TurnManager TurnManager;
    [SerializeField]
    private ChainManager ChainManager;
    [SerializeField]
    private SecondaryUIManager UIManager;
    [SerializeField]
    private MainUIManager MainUIManager;
    [SerializeField]
    private DeckManager DeckManager;
    [SerializeField]
    private StatusManager StatusManager;
    [SerializeField]
    private AudioManager AudioManager;

    public ToolTipManager ToolTipManager;

    public PlayerManager BluePlayerManager, RedPlayerManager, turnPlayer, turnOpponent;

    public bool isNotFirstDraw, hasFinishedDrawEffect, isPlayingCard, isActivatingEffect, isChecking, isWaitingForResponse, isShowingInfo, isShowingLog;

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
    public string CardDatabasePath, GodDatabasePath, BluePlayerPath, RedPlayerPath, ToolTipsDatabasePath;

    public float loadStartTime;
    public float loadEndTime;

#pragma warning disable IDE0051 // Remove unused private members
    private void Start()
    {
#pragma warning restore IDE0051 // Remove unused private members
        loadStartTime = Time.realtimeSinceStartup;
        AudioManager.FindBGOBJ();
        AudioManager.FindBattleOBJ();
        AudioManager.SelectRandomBGMusic();
        AudioManager.LoadSFX();
        LoadPlayers(BluePlayerManager, BluePlayerPath);
        LoadPlayers(RedPlayerManager, RedPlayerPath);
        LoadToolTips();

        //after it's back from the cross country hike, we can move on
        popUpPanelText = popUpPanel.GetComponentInChildren<TMP_Text>();
        phaseChangeButtonText = phaseChangeButton.GetComponentInChildren<TMP_Text>();
        UIManager.UIUpdate(BluePlayerManager);
        UIManager.UIUpdate(RedPlayerManager);
        BluePlayerManager.SetShield(0, BluePlayerManager.shieldCount);
        RedPlayerManager.SetShield(0, RedPlayerManager.shieldCount);
        foreach (CardSlot slot in Row1)
        {
            slot.InitializeSlot(this);
            slot.ChangeController(RedPlayerManager);
        }
        RedPlayerManager.cardSlots = new(Row1);
        foreach (CardSlot slot in Row2)
        {
            slot.InitializeSlot(this);
            slot.ChangeController(RedPlayerManager);
            slot.isFrontline = true;
        }
        RedPlayerManager.cardSlots.AddRange(Row2);
        foreach (CardSlot slot in Row3)
        {
            slot.InitializeSlot(this);
            slot.ChangeController(BluePlayerManager);
            slot.isFrontline = true;
        }
        BluePlayerManager.cardSlots = new(Row3);
        foreach (CardSlot slot in Row4)
        {
            slot.InitializeSlot(this);
            slot.ChangeController(BluePlayerManager);
        }
        BluePlayerManager.cardSlots.AddRange(Row4);
        for(int i = 0; i < BluePlayerManager.hand.transform.childCount; i++)
            BluePlayerManager.handSlots.Add(BluePlayerManager.hand.transform.GetChild(i).GetComponent<HandSlot>());
        for (int i = 0; i < RedPlayerManager.hand.transform.childCount; i++)
            RedPlayerManager.handSlots.Add(RedPlayerManager.hand.transform.GetChild(i).GetComponent<HandSlot>());
        loadEndTime = Time.realtimeSinceStartup;
        Debug.Log($"Card Load time is : {loadEndTime - loadStartTime} seconds");
        StartCoroutine(TurnManager.ChooseFirstPlayer());
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

    public void LoadToolTips() => ToolTipManager.tooltipInfos.AddRange(SaveManager.LoadToolTipInfoDatabase(ToolTipsDatabasePath));

    public IEnumerator DrawCard(int drawAmount, PlayerManager player)
    {
        hasFinishedDrawEffect = false;
        //if hand is full, yeet
        if (player.handSize >= 10)
        {
            hasFinishedDrawEffect = true;
            yield break;
        }

        bool drewCards = false;
        foreach (HandSlot handSlot in player.handSlots)
        {
            //if deck is empty, has drawn required amount or hand is full, break
            if (player.deckLogicList.Count <= 0 || drawAmount <= 0 || player.handSize >= 10)
                break;
            //checking for empty handslots
            if (handSlot.cardInZone != null)
                continue;
            //get random card and activate it
            int randomNumber = Random.Range(0, player.deckLogicList.Count);
            CardLogic randomCardDraw = player.deckLogicList[randomNumber];
            randomCardDraw.gameObject.SetActive(true);

            //implementing a game log
            randomCardDraw.LocationChange(Location.Hand, player.handSize);
            randomCardDraw.transform.SetParent(handSlot.transform, false);
            randomCardDraw.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            AudioSource drawSound = AudioManager.NewAudioPrefab(AudioManager.draw);
            //when playing with another player on same device flip face up only if you draw on your turn...
            //might implement more to support this
            if (player.isLocal && !player.isAI && (player == turnPlayer || player.enemy.isAI || !player.enemy.isLocal))
                randomCardDraw.FlipFaceUp();
          
            handSlot.cardInZone = randomCardDraw;
            player.deckLogicList.Remove(randomCardDraw);
            player.handLogicList.Add(randomCardDraw);
            drawAmount--;
            player.handSize = player.handLogicList.Count;
            drewCards = true;
            yield return new WaitUntil(() => drawSound == null);
        }
        ShuffleHand(player);
        //ensures that unnecessary chains and shuffles don't occur on unresolved draws
        hasFinishedDrawEffect = true;
        if (drewCards)
        {
            if (isNotFirstDraw)
                StateChange(GameState.Reinforcement);
            if (currentFocusCardLogic != null && isActivatingEffect)
                currentFocusCardLogic.FinishResolution(currentFocusCardLogic.focusSubEffect);
            else
                ChainResolution();
        }
        yield break;
    }

    public IEnumerator SearchCard(CardLogic logic, PlayerManager player)
    {
        if (player.handSize >= 10)
            yield break;

        foreach (HandSlot handSlot in player.handSlots)
        {
            if (handSlot.cardInZone != null)
                continue;
            if (player.handSize >= 10)
                break;
            logic.gameObject.SetActive(true);

            //implementing a battle log
            logic.LocationChange(Location.Hand, player.handSize);
            logic.transform.SetParent(handSlot.transform, false);
            logic.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            //when playing with another player on same device flip face up only if you draw on your turn...
            //might implement more to support this
            if (player.isLocal && !player.isAI && (player == turnPlayer || player.enemy.isAI || !player.enemy.isLocal))
                logic.FlipFaceUp();
            handSlot.cardInZone = logic;
            player.deckLogicList.Remove(logic);
            player.handLogicList.Add(logic);
            player.handSize = player.handLogicList.Count;
            break;
        }
        AudioSource drawSound = AudioManager.NewAudioPrefab(AudioManager.draw);
        yield return new WaitUntil(() => drawSound == null);

        ShuffleHand(player);
        StateChange(GameState.Reinforcement);
        yield break;
    }

    public IEnumerator RecoverCard(CardLogic logic, PlayerManager player)
    {
        if (player.handSize >= 10)
            yield break;

        foreach (HandSlot handSlot in player.handSlots)
        {
            if (handSlot.cardInZone != null)
                continue;
            if (player.handSize >= 10)
                break;
            logic.gameObject.SetActive(true);

            //implementing a battle log
            logic.LocationChange(Location.Hand, player.handSize);

            logic.ControllerSwap(player);
            logic.transform.SetParent(handSlot.transform, false);
            logic.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            //when playing with another player on same device flip face up only if you draw on your turn...
            //might implement more to support this
            if (player.isLocal && !player.isAI && (player == turnPlayer || player.enemy.isAI || !player.enemy.isLocal))
                logic.FlipFaceUp();
            handSlot.cardInZone = logic;
            logic.cardOwner.graveLogicList.Remove(logic);
            player.handLogicList.Add(logic);
            player.handSize = player.handLogicList.Count;
            break;
        }
        AudioSource drawSound = AudioManager.NewAudioPrefab(AudioManager.draw);
        yield return new WaitUntil(() => drawSound == null);

        ShuffleHand(player);
        StateChange(GameState.Reinforcement);
        yield break;
    }

    public IEnumerator GetShieldCard(int drawAmount, PlayerManager player)
    {
        if (player.handSize >= 10)
            yield break;

        foreach (HandSlot handSlot in player.handSlots)
        {
            if (handSlot.cardInZone != null)
                continue;
            if (player.heroDeckLogicList.Count <= 0 || drawAmount <= 0 || player.handSize >= 10)
                break;
            int randomNumber = UnityEngine.Random.Range(0, player.heroDeckLogicList.Count);
            CardLogic randomCardDraw = player.heroDeckLogicList[randomNumber];
            randomCardDraw.gameObject.SetActive(true);

            //implementing a battle log
            randomCardDraw.LocationChange(Location.Hand, player.handSize);
            randomCardDraw.transform.SetParent(handSlot.transform, false);
            randomCardDraw.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            //when playing with another player on same device flip face up only if you draw on your turn...
            //might implement more to support this
            if (player.isLocal && !player.isAI && (player == turnPlayer || player.enemy.isAI || !player.enemy.isLocal))
                randomCardDraw.FlipFaceUp();
            handSlot.cardInZone = randomCardDraw;
            player.heroDeckLogicList.Remove(randomCardDraw);
            player.handLogicList.Add(randomCardDraw);
            drawAmount--;
            player.handSize = player.handLogicList.Count;

        }
        AudioSource drawSound = AudioManager.NewAudioPrefab(AudioManager.draw);
        yield return new WaitUntil(() => drawSound == null);

        ShuffleHand(player);
        StateChange(GameState.Reinforcement);
        if (!isWaitingForResponse)
            ChainResolution();
        yield break;
    }


    public void ShuffleHand(PlayerManager player)
    {
        AudioManager.NewAudioPrefab(AudioManager.shuffleHand);
        //resets hand to zero transform and empty
        foreach (HandSlot handSlot in player.handSlots)
        {
            handSlot.transform.localPosition = Vector3.zero;
            handSlot.cardInZone = null;
        }

        //gets list of all cards in player hand then set parent to null and hold a reference to it;
        System.Random random = new();
        IOrderedEnumerable<CardLogic> shuffledHandCards = player.handLogicList.OrderBy(x => random.Next());
        List<CardLogic> handCards = new(shuffledHandCards);
        player.handLogicList.Clear();
        foreach (CardLogic logic in handCards)
            logic.transform.SetParent(null);
        ReattachShuffledHand(player, handCards);
        ArrangeHand(player);
    }

    private static void ArrangeHand(PlayerManager player)
    {
        //do not fuck with this, DO NOT FUCK WITH THIS!
        float xDist = 2.1f;
        float yDist = -0.82f;
        float cardSize = 0.511f;
        int count = 0;
        int topCount = Mathf.FloorToInt(player.handSize / 2 + 0.5f);
        int bottomCount = player.handSize - topCount;

        foreach (HandSlot handSlot in player.handSlots)
        {
            if (handSlot.cardInZone == null)
                continue;
            count++;
            int trueCount = count - topCount;
            float yPosition = player.handSize < 4 ? yDist / 2 : count <= player.handSize / 2 ? 0 : yDist;
            float xPosition = player.handSize < 4
                ? xDist / (player.handSize + 1) * count
                : (trueCount <= 0 && topCount < 4) || (trueCount > 0 && bottomCount < 4)
                    ? trueCount <= 0 ? xDist / (topCount + 1) * count : xDist / (bottomCount + 1) * trueCount
                    : count == 1 || trueCount == 1 ? 0 : trueCount <= 0 ? cardSize * (count - 1) : cardSize * (trueCount - 1);
            if (trueCount <= 0 && topCount == 4 || trueCount > 0 && bottomCount == 4)
                xPosition += 0.11f * (trueCount <= 0 ? count : trueCount);
            handSlot.transform.localPosition += new Vector3(xPosition, yPosition, 0);
        }
    }

    private static void ReattachShuffledHand(PlayerManager player, List<CardLogic> handCards)
    {
        //reattaches cards to handslots
        foreach (CardLogic logic in handCards)
        {
            foreach (HandSlot handSlot in player.handSlots)
            {
                if (handSlot.cardInZone != null)
                    continue;
                logic.locationOrderNumber = player.handSize;
                logic.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                logic.transform.localScale = new(4, 3, 1);
                logic.transform.SetParent(handSlot.transform, false);
                handSlot.cardInZone = logic;
                player.handLogicList.Add(logic);
                player.handSize = player.handLogicList.Count;
                break;
            }
        }
    }

    public void StateReset() {
        StateChange(GameState.Open);
        if(currentFocusCardLogic != null) 
            currentFocusCardLogic.RemoveFocusCardLogic();
        if (currentFocusCardSlot != null)
            currentFocusCardSlot.DeselectSlot();
        if (currentPhase == Phase.MainPhase)
            ShowValidMoves(turnPlayer);
        if (currentPhase == Phase.BattlePhase)
            ShowValidAttackers(turnPlayer);
        if(turnPlayer.isAI)
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
                return;
        }
        ChainManager.GetEmptyStateTriggers(state);
        if (currentFocusCardLogic == null)
            return;
        GetStateTriggers(currentFocusCardLogic, state);
    }

    public void PhaseChange(Phase phase)
    {
        currentPhase = phase;
        GetPhaseTriggers(phase);
        ChainResolution();
    }

    public void GetPhaseTriggers(Phase phase) => ChainManager.GetPhaseTriggers(phase);

    public void GetEffectTriggers(SubEffect subEffect, CardLogic triggerCard)=>
        ChainManager.GetEffectTriggers(subEffect, triggerCard);

    public void GetStateTriggers(CardLogic triggerCard, GameState state)=>
        ChainManager.GetStateTriggers(triggerCard, state);

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
        ChainManager.ChainResolution();
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
            cardLogic.EffectRefresh();
        foreach (CardLogic cardLogic in player.fieldLogicList)
        {
            cardLogic.EffectRefresh();
            if (cardLogic.type == Type.Fighter)
                cardLogic.GetComponent<MonsterLogic>().hasMoved = false;
        }
        foreach (CardLogic cardLogic in player.graveLogicList)
            cardLogic.EffectRefresh();
        player.heroCardLogic.EffectRefresh();
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
                cardLogic.FlipFaceDown();
        foreach (CardLogic cardLogic in player.handLogicList)
            cardLogic.FlipFaceUp();
    }

    public void ShowValidMoves(PlayerManager player)
    {
        player.playableLogicList.Clear();
        player.canUseEffectLogicList.Clear();
        player.canUseEffectList.Clear();
        player.canUseSubEffectList.Clear();
        foreach (CardLogic cardLogic in player.handLogicList)
        {
            if (cardLogic.GetComponent<PlayableLogic>().LegalPlayCheck(false, player) != null)
            {
                cardLogic.GreyScaleEffect();
                continue;
            }
            player.playableLogicList.Add(cardLogic);
            cardLogic.NormalColour();
        }
        foreach (CardLogic cardLogic in player.fieldLogicList)
        {
            if (cardLogic.GetComponent<CombatantLogic>().ImmobilityCheck())
                continue;
            foreach (Effect effect in cardLogic.effects)
            {
                foreach (SubEffect subEffect in effect.SubEffects)
                {
                    if (subEffect.effectType != EffectTypes.Deployed)
                        continue;
                  
                    if (effect.currentActivations >= effect.maxActivations)
                        continue;
                    if (cardLogic.GetValidTargets(subEffect).Count == 0)
                        continue;
                    player.canUseEffectLogicList.Add(cardLogic);
                    player.canUseEffectList.Add(effect);
                    player.canUseSubEffectList.Add(subEffect);
                }
            }
        }
        foreach (Effect effect in player.heroCardLogic.effects)
        {
            foreach (SubEffect subEffect in effect.SubEffects)
            {
                if (subEffect.effectType != EffectTypes.Deployed)
                    continue;
                if (player.heroCardLogic.GetValidTargets(subEffect).Count == 0)
                    continue;
                player.canUseEffectLogicList.Add(player.heroCardLogic);
                player.canUseEffectList.Add(effect);
                player.canUseSubEffectList.Add(subEffect);
            }
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
}