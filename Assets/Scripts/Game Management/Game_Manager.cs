using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static CardLogic;

public class Game_Manager : MonoBehaviour
{
    [SerializeField]
    private SaveManager SaveManager;
    [SerializeField]
    private TurnManager TurnManager;
    [SerializeField]
    private ChainManager ChainManager;
    [SerializeField]
    private UIManager UIManager;
    [SerializeField]
    private UXManager UXManager;
    [SerializeField]
    private DeckManager DeckManager;

    public PlayerManager BluePlayerManager, RedPlayerManager, turnPlayer, turnOpponent;

    public bool isNotLoading, isActivatingEffect, isChecking, isWaitingForResponse;

    public enum GameState
    {
        Open, Damaged, Death, Grave, Summon, Activation, EffectActivation, Targeting, Reinforcement, Cost, ChainResolution, Playing, Deployment, Revive, EffectResolution, AttackDeclaration, Shielded, TurnEnd, Undefined
    }
    public enum Phase
    {
        DrawPhase, CostPhase, MainPhase, BattlePhase, EndPhase, Undefined
    }

    public GameState gameState;
    public Phase currentPhase;
    public List<CardLogic> activationChainList = new();
    public List<int> activationChainNumber = new();
    public List<int> activationChainSubNumber = new();

    public CardLogic currentFocusCardLogic;
    public GameObject phaseChangeButton;

    public TMP_Text bluePlayerText, redPlayerText, turnCountText, turnPhaseText;

    public int turnCount;
    public string DatabasePath, BluePlayerPath, RedPlayerPath;

    void Start()
    {
        LoadPlayers(BluePlayerManager, BluePlayerPath);
        LoadPlayers(RedPlayerManager, RedPlayerPath);
        //after it's back from the cross country hike, we can move on
        TurnManager.ChooseFirstPlayer();
    }

    void Update()
    {
        UIManager.UIUpdate(BluePlayerManager);
        UIManager.UIUpdate(RedPlayerManager);
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
        playerManager.deckLogicList.AddRange(DeckManager.LoadDeck(playerManager.deckListID, playerManager.deckList, playerManager.deck, playerManager, false));
        playerManager.heroDeckLogicList.AddRange(DeckManager.LoadDeck(playerManager.heroDeckListID, playerManager.heroDeckList, playerManager.shield, playerManager, true));
    }

    public void CostChange(int changeAmount, PlayerManager player, bool add)
    {
        if (add)
            player.costCount += changeAmount;
        else
            player.costCount -= changeAmount;
        if (player.costCount > 10)
            player.costCount = 10;
        if (player.costCount < 0)
            player.costCount = 0;
        gameState = GameState.Cost;
    }

    public void DrawCard(int drawAmount, PlayerManager player)
    {
        if (player.handSize >= 10)
            return;

        for (int i = 0; i < player.isEmptyHandSlot.Length; i++)
        {
            if (player.isEmptyHandSlot[i] == false)
                continue;
            if (player.deckLogicList.Count <= 0)
                break;
            if (drawAmount <= 0)
                break;
            if (player.handSize >= 10)
                break;
            int randomNumber = Random.Range(0, player.deckLogicList.Count);
            CardLogic randomCardDraw = player.deckLogicList[randomNumber];
            randomCardDraw.gameObject.SetActive(true);
            randomCardDraw.currentLocation = CardLogic.Location.Hand;
            randomCardDraw.locationOrderNumber = i;
            randomCardDraw.transform.position = Vector3.zero;

            //implementing a battle log
            if (isActivatingEffect)
                randomCardDraw.locationHistoryEntries.Add(new LocationHistoryEntry(currentFocusCardLogic, Effect.EffectsUsed.Reinforce, Location.Hand, Location.Deck));
            else
                randomCardDraw.locationHistoryEntries.Add(new LocationHistoryEntry(null, Effect.EffectsUsed.Undefined, Location.Hand, Location.Deck));
            randomCardDraw.transform.SetParent(player.handSlots[i].transform, false);
            //when playing with another player on same device flip face up only if you draw on your turn...might implement more to support this
            if (player.isLocal && !player.isAI && (player == turnPlayer || player.enemy.isAI || !player.enemy.isLocal))
                randomCardDraw.FlipFaceUp();
          
            player.isEmptyHandSlot[i] = false;
            player.deckLogicList.Remove(randomCardDraw);
            player.handLogicList.Add(randomCardDraw);
            drawAmount--;
            player.handSize = player.handLogicList.Count;
        }
        ShuffleHand(player);
        if (isNotLoading)
            StateChange(GameState.Reinforcement);
        if (currentPhase == Phase.DrawPhase)
            ChainResolution();
    }

    public void SearchCard(CardLogic logic, PlayerManager player)
    {
        if (player.handSize >= 10)
            return;

        for (int i = 0; i < player.isEmptyHandSlot.Length; i++)
        {
            if (player.isEmptyHandSlot[i] == false)
                continue;
            if (player.handSize >= 10)
                break;
            logic.gameObject.SetActive(true);
            logic.currentLocation = CardLogic.Location.Hand;
            logic.locationOrderNumber = i;
            logic.transform.position = Vector3.zero;

            //implementing a battle log
            logic.locationHistoryEntries.Add(new LocationHistoryEntry(currentFocusCardLogic, Effect.EffectsUsed.Recruit, Location.Hand, Location.Deck));
            logic.transform.SetParent(player.handSlots[i].transform, false);
            //when playing with another player on same device flip face up only if you draw on your turn...might implement more to support this
            if (player.isLocal && !player.isAI && (player == turnPlayer || player.enemy.isAI || !player.enemy.isLocal))
                logic.FlipFaceUp();
            player.isEmptyHandSlot[i] = false;
            player.deckLogicList.Remove(logic);
            player.handLogicList.Add(logic);
            player.handSize = player.handLogicList.Count;
            break;
        }
        ShuffleHand(player);
        StateChange(GameState.Reinforcement);
    }

    public void RecoverCard(CardLogic logic, PlayerManager player)
    {
        if (player.handSize >= 10)
            return;

        for (int i = 0; i < player.isEmptyHandSlot.Length; i++)
        {
            if (player.isEmptyHandSlot[i] == false)
                continue;
            if (player.handSize >= 10)
                break;
            logic.gameObject.SetActive(true);
            logic.currentLocation = CardLogic.Location.Hand;
            logic.locationOrderNumber = i;
            logic.transform.position = Vector3.zero;
            //implementing a battle log
            logic.locationHistoryEntries.Add(new LocationHistoryEntry(currentFocusCardLogic, Effect.EffectsUsed.Recover, Location.Hand, Location.Deck));
            logic.ControllerSwap(player);
            logic.transform.SetParent(player.handSlots[i].transform, false);
            //when playing with another player on same device flip face up only if you draw on your turn...might implement more to support this
            if (player.isLocal && !player.isAI && (player == turnPlayer || player.enemy.isAI || !player.enemy.isLocal))
                logic.FlipFaceUp();
            player.isEmptyHandSlot[i] = false;
            logic.cardOwner.graveLogicList.Remove(logic);
            player.handLogicList.Add(logic);
            player.handSize = player.handLogicList.Count;
            break;
        }
        ShuffleHand(player);
        StateChange(GameState.Reinforcement);
    }

    public void GetShieldCard(int drawAmount, PlayerManager player)
    {
        if (player.handSize >= 10)
            return;

        for (int i = 0; i < player.isEmptyHandSlot.Length; i++)
        {
            if (player.isEmptyHandSlot[i] == false)
                continue;
            if (player.heroDeckLogicList.Count <= 0)
                break;
            if (drawAmount <= 0)
                break;
            if (player.handSize >= 10)
                break;
            int randomNumber = Random.Range(0, player.heroDeckLogicList.Count);
            CardLogic randomCardDraw = player.heroDeckLogicList[randomNumber];
            randomCardDraw.gameObject.SetActive(true);
            randomCardDraw.currentLocation = CardLogic.Location.Hand;
            randomCardDraw.locationOrderNumber = i;
            randomCardDraw.transform.position = Vector3.zero;

            //implementing a battle log
            //effect is still undefined
            if (isActivatingEffect)
                randomCardDraw.locationHistoryEntries.Add(new LocationHistoryEntry(currentFocusCardLogic, Effect.EffectsUsed.Undefined, Location.Hand, Location.Deck));
            else
                randomCardDraw.locationHistoryEntries.Add(new LocationHistoryEntry(null, Effect.EffectsUsed.Undefined, Location.Hand, Location.Deck));
            randomCardDraw.transform.SetParent(player.handSlots[i].transform, false);
            //when playing with another player on same device flip face up only if you draw on your turn...might implement more to support this
            if (player.isLocal && !player.isAI && (player == turnPlayer || player.enemy.isAI || !player.enemy.isLocal))
                randomCardDraw.FlipFaceUp();
            player.isEmptyHandSlot[i] = false;
            player.heroDeckLogicList.Remove(randomCardDraw);
            player.handLogicList.Add(randomCardDraw);
            drawAmount--;
            player.handSize = player.handLogicList.Count;

        }
        ShuffleHand(player);
        StateChange(GameState.Reinforcement);
        if (!isWaitingForResponse)
            ChainResolution();
    }


    public void ShuffleHand(PlayerManager player)
    {
        //resets hand to zero transform and empty
        for (int i = 0; i < player.handSlots.Length; i++)
        {
            player.handSlots[i].transform.localPosition = Vector3.zero;
            player.isEmptyHandSlot[i] = true;
        }

        //gets list of all cards in player hand then set parent to null and hold a reference to it;

        List<CardLogic> handCards = new(player.handLogicList);
        foreach(CardLogic logic in handCards)
            logic.transform.SetParent(null);

        //reattaches cards to handslots
        foreach (CardLogic logic in handCards)
        {
            for (int i = 0; i < player.handSlots.Length; i++)
            {
                if (player.isEmptyHandSlot[i] == true)
                {
                    logic.locationOrderNumber = i;
                    logic.transform.position = Vector3.zero;
                    logic.transform.SetParent(player.handSlots[i].transform, false);
                    player.isEmptyHandSlot[i] = false;
                    break;
                }
            }
        }

        //gets reference to all taken slots
        int[] handSlotsTaken = player.isEmptyHandSlot.FindAllIndexof<bool>(false);
        //if list isn't empty, arrange handslots
        if (handSlotsTaken != null)
        {
            for(int i = 0;i< handSlotsTaken.Length;i++)
            {
                    float xPosition = 15f / handSlotsTaken.Length;
                    player.handSlots[handSlotsTaken[i]].transform.localPosition += new Vector3(i * xPosition, 0, 0);
            }
        }
        player.handSize = player.handLogicList.Count;
    }

    public void StateReset() {
        StateChange(GameState.Open);
        if (currentPhase == Phase.MainPhase)
            ShowValidMoves(turnPlayer);
        if (currentPhase == Phase.BattlePhase)
            ShowValidAttackers(turnPlayer);
        if(turnPlayer.isAI)
           turnPlayer.AIManager.MakeDecision();
    }

    public void StateChange(GameState state)
    {
        gameState = state;
        if (gameState == GameState.Open)
            return;
        if (gameState == GameState.EffectActivation)
            return;
        if (gameState == GameState.Targeting)
            return;
        if (gameState == GameState.ChainResolution)
            return;
        ChainManager.GetEmptyStateTriggers();
        if (currentFocusCardLogic == null)
            return;
        GetStateTriggers(currentFocusCardLogic);
    }

    public void PhaseChange(Phase phase)
    {
        currentPhase = phase;
        GetPhaseTriggers(phase);
    }

    public void GetPhaseTriggers(Phase phase) => ChainManager.GetPhaseTriggers(phase);

    public void GetEffectTriggers(int triggerEffectNumber, int triggerSubEffectNumber, CardLogic triggerCard)
    {
        ChainManager.GetEffectTriggers(triggerEffectNumber, triggerSubEffectNumber, triggerCard);
    }

    public void GetStateTriggers(CardLogic triggerCard)
    {
        ChainManager.GetStateTriggers(triggerCard);
    }

    public void ChainResolution()
    {
        //nothing to do if in middle of effect
        if (isActivatingEffect)
            return;
        if (isWaitingForResponse)
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

    public void DisableTurnUI()
    {
        phaseChangeButton.SetActive(false);
        turnPhaseText.gameObject.SetActive(false);
        turnCountText.gameObject.SetActive(false);
    }

    public void EnableTurnUI()
    {
        phaseChangeButton.SetActive(true);
        turnPhaseText.gameObject.SetActive(true);
        turnCountText.gameObject.SetActive(true);
    }

    public void DisableRayBlocker() => UXManager.DisableRayBlocker();

    public void ShowEffectInfoPanel() => UXManager.ShowEffectInfoPanel();

    public void DisableEffectInfoPanels() => UXManager.DisableEffectInfoPanels();

    public void EnableActivationPanel() => UXManager.EnableEffectActivationPanel();

    public void ShowShieldPrompt(PlayerManager cardOwner) => UXManager.ShowShieldPrompt(cardOwner);

    public void AllEffectsRefresh(PlayerManager player)
    {
        foreach (CardLogic cardLogic in player.handLogicList)
            cardLogic.EffectRefresh();
        foreach (CardLogic cardLogic in player.fieldLogicList)
            cardLogic.EffectRefresh();
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

    public void ShieldRefresh(PlayerManager player) => player.heroCardLogic.ShieldRefresh();

    public void SwitchControl(PlayerManager player)
    {
        if (player.isAI)
            return;
        if (!player.isLocal)
            return;
        if (player.enemy.isAI)
            return;
        if (!player.enemy.isLocal)
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
        player.canUseEffectNumber.Clear();
        player.canUseEffectSubNumber.Clear();
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
            foreach (Effect effect in cardLogic.effects)
            {
                if (!effect.EffectType.Contains("Deployed"))
                    continue;
                    int subNum = effect.EffectType.FindIndex(a => a == "Deployed");
                int effNum = cardLogic.effects.FindIndex(a => a == effect);
                if (cardLogic.effects[effNum].currentActivations >= cardLogic.effects[effNum].maxActivations)
                    continue;
                if (cardLogic.GetValidTargets(effNum, subNum).Count == 0)
                    continue;
                player.canUseEffectLogicList.Add(cardLogic);
                player.canUseEffectNumber.Add(effNum);
                player.canUseEffectSubNumber.Add(subNum);

            }
        }
        foreach (Effect effect in player.heroCardLogic.effects)
        {
            if (!effect.EffectType.Contains("Deployed"))
                continue;
            int subNum = effect.EffectType.FindIndex(a => a == "Deployed");
            int effNum = player.heroCardLogic.effects.FindIndex(a => a == effect);
            if (player.heroCardLogic.GetValidTargets(effNum, subNum).Count == 0)
                continue;
            player.canUseEffectLogicList.Add(player.heroCardLogic);
            player.canUseEffectNumber.Add(effNum);
            player.canUseEffectSubNumber.Add(subNum);
        }
    }

    public void ShowValidAttackers(PlayerManager player)
    {
        player.canAttackLogicList.Clear();
        foreach (CardLogic cardLogic in player.fieldLogicList)
        {
            CombatantLogic combatantLogic = cardLogic.GetComponent<CombatantLogic>();
            if (combatantLogic.attacksLeft > 0 && combatantLogic.currentAtk > 0)
                player.canAttackLogicList.Add(cardLogic);
        }
        //implement atk for god???
    }

    public void ClearEffectTargetImages()
    {
        foreach (GameObject image in BluePlayerManager.effectTargets)
            image.SetActive(false);
        foreach (GameObject image in RedPlayerManager.effectTargets)
            image.SetActive(false);
        RedPlayerManager.heroEffectTarget.SetActive(false);
        RedPlayerManager.deckTarget.SetActive(false);
        RedPlayerManager.graveTarget.SetActive(false);
        BluePlayerManager.heroEffectTarget.SetActive(false);
        BluePlayerManager.deckTarget.SetActive(false);
        BluePlayerManager.graveTarget.SetActive(false);
    }

    public void GameOver(PlayerManager winner)
    {
        UXManager.GameOver(winner);
        isNotLoading = false;
    }

    public void ClearAttackTargetImages()
    {
        foreach (GameObject image in BluePlayerManager.attackTargets)
            image.SetActive(false);
        foreach (GameObject image in RedPlayerManager.attackTargets)
            image.SetActive(false);
        RedPlayerManager.heroAttackTarget.SetActive(false);
        BluePlayerManager.heroAttackTarget.SetActive(false);
    }

    public void EnableCardScrollScreen(List<CardLogic> cardLogics, bool shouldShowButton) =>
        UXManager.EnableCardScrollScreen(cardLogics, shouldShowButton);

    public void DisableCardScrollScreen() => UXManager.DisableCardScrollScreen();
}