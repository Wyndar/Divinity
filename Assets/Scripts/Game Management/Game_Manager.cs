using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    public bool isNotLoading;

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
        LoadDeckID(playerManager);
    }

    public void LoadDeckID(PlayerManager playerManager)
    {
        playerManager.deckListID.AddRange(SaveManager.LoadIDFromJson(playerManager.DeckPath));
        playerManager.heroDeckListID.AddRange(SaveManager.LoadIDFromJson(playerManager.HeroDeckPath));
        playerManager.deckLogicList.AddRange(DeckManager.LoadDeck(playerManager.deckListID, playerManager.deckList, playerManager.deck, playerManager, false));
        playerManager.heroDeckLogicList.AddRange(DeckManager.LoadDeck(playerManager.heroDeckListID, playerManager.heroDeckList, playerManager.shield, playerManager, true));
    }

    public void CostChange(int changeAmount, PlayerManager player)
    {
        player.costCount += changeAmount;
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
            int randomNumber = Random.Range(0, player.deckLogicList.Count);
            CardLogic randomCardDraw = player.deckLogicList[randomNumber];
            randomCardDraw.gameObject.SetActive(true);
            randomCardDraw.currentLocation = CardLogic.Location.Hand;
            randomCardDraw.locationOrderNumber = i;
            randomCardDraw.transform.position = Vector3.zero;
            randomCardDraw.transform.SetParent(player.handSlots[i].transform, false);
            if (player.isLocal && player.isAI == false && player == turnPlayer)
                randomCardDraw.FlipFaceUp();
            player.isEmptyHandSlot[i] = false;
            player.deckLogicList.Remove(randomCardDraw);
            drawAmount--;
            player.handSize++;
        }
        ShuffleHand(player);
        if(isNotLoading)
            StateChange(GameState.Reinforcement);
        if(currentPhase == Phase.DrawPhase)
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
            if (player.deckLogicList.Count <= 0)
                break;
            logic.gameObject.SetActive(true);
            logic.currentLocation = CardLogic.Location.Hand;
            logic.locationOrderNumber = i;
            logic.transform.position = Vector3.zero;
            logic.transform.SetParent(player.handSlots[i].transform, false);
            if (player.isLocal && player.isAI == false && player == turnPlayer)
                logic.FlipFaceUp();
            player.isEmptyHandSlot[i] = false;
            player.deckLogicList.Remove(logic);
            player.handSize++;
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
            int randomNumber = Random.Range(0, player.heroDeckLogicList.Count);
            CardLogic randomCardDraw = player.heroDeckLogicList[randomNumber];
            randomCardDraw.gameObject.SetActive(true);
            randomCardDraw.currentLocation = CardLogic.Location.Hand;
            randomCardDraw.locationOrderNumber = i;
            randomCardDraw.transform.position = Vector3.zero;
            randomCardDraw.transform.SetParent(player.handSlots[i].transform, false);
            if (player.isLocal && player.isAI == false && player == turnPlayer)
                randomCardDraw.FlipFaceUp();
            player.isEmptyHandSlot[i] = false;
            player.heroDeckLogicList.Remove(randomCardDraw);
            drawAmount--;
            player.handSize++;
        }
        ShuffleHand(player);
        StateChange(GameState.Reinforcement);
    }

    public void ShuffleHand(PlayerManager player)
    {
        //resets hand to zero transform and empty
        for (int i = 0; i<player.handSlots.Length; i++)
        {
            player.handSlots[i].transform.localPosition = Vector3.zero;
            player.isEmptyHandSlot[i] = true;
        }

        //gets list of all cards to filter for ones in player hand then set parent to null and hold a reference to it;
        List<CardLogic> cardLogics = new(FindObjectsOfType<CardLogic>());
        List<CardLogic> handCards = new();
        foreach (CardLogic cardLogic in cardLogics)
        {
            if (cardLogic.cardOwner != player)
                continue;
            if (cardLogic.currentLocation != CardLogic.Location.Hand)
                continue;
            cardLogic.transform.SetParent(null);
            handCards.Add(cardLogic);
        }

        //reattaches cards to handslots
        foreach(CardLogic logic in handCards)
        {
            for(int i = 0; i< player.handSlots.Length;i++)
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
        List<int> handSlotsTaken = new();
        for (int i = 0; i < player.isEmptyHandSlot.Length; i++)
        {
            if (player.isEmptyHandSlot[i] == false)
                handSlotsTaken.Add(i);
        }
        //if list isn't empty, arrange handslots
        if (handSlotsTaken != null)
        {
            for (int i = 0; i < handSlotsTaken.Count; i++)
            {
                if (player.handSlots[handSlotsTaken[i]] != null)
                {
                    float xPosition = 15f / handSlotsTaken.Count;
                    player.handSlots[handSlotsTaken[i]].transform.localPosition += new Vector3(i * xPosition, 0, 0);
                }
            }
        }
    }

    public void StateReset() => StateChange(GameState.Open);

    public void StateChange(GameState state)
    {
        gameState = state;
        if (gameState == GameState.Open)
            return;
        if(gameState == GameState.EffectActivation)
            return;
        if(gameState == GameState.Targeting)
            return;
        if(gameState == GameState.ChainResolution)
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
        StateReset();
    }

    public void GetStateTriggers(CardLogic triggerCard)
    {
        ChainManager.GetStateTriggers(triggerCard);
        StateReset();
    }

    public void ChainResolution()
    {
        StateReset();
        if (activationChainList.Count == 0)
            return;
        else
            ChainManager.ChainResolution();
        StateReset();
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

    public void EnableActivationPanel() => UXManager.EnableEffectActivationPanel();

    public void DisableActivationPanel() => UXManager.DisableEffectActivationPanel();

    public void ShowShieldPrompt(PlayerManager cardOwner) => UXManager.ShowShieldPrompt(cardOwner);

    public void AllEffectsRefresh(PlayerManager player)
    {
        List<CardLogic> cardLogics = new(FindObjectsOfType<CardLogic>());
        foreach (CardLogic cardLogic in cardLogics)
        {
            if (player != cardLogic.cardOwner)
                continue;
            if (cardLogic.currentLocation == CardLogic.Location.Deck || cardLogic.currentLocation == CardLogic.Location.HeroDeck)
                continue;
            cardLogic.EffectRefresh();
        }
    }

    public void AllAttacksRefresh(PlayerManager player)
    {
        List<CombatantLogic> combatantLogics = new(FindObjectsOfType<CombatantLogic>());
        foreach (CombatantLogic combatantLogic in combatantLogics)
        {
            if (combatantLogic.logic.cardOwner != player)
                continue;
            if (combatantLogic.logic.currentLocation != CardLogic.Location.Field)
                continue;
            combatantLogic.AttackRefresh();
            combatantLogic.hasAttackedThisTurn = false;
        }
    }

    public void ShieldRefresh(PlayerManager player)
    {
        List<GodLogic> godLogics = new(FindObjectsOfType<GodLogic>());
        foreach(GodLogic godLogic in godLogics)
            if (godLogic.cardOwner == player)
                godLogic.ShieldRefresh();
    }

    public void SwitchControl(PlayerManager player)
    {
        if (player.isAI)
            return;
        if (!player.isLocal)
            return;
        List<CardLogic> cardLogics = new(FindObjectsOfType<CardLogic>());
        foreach (CardLogic cardLogic in cardLogics)
        { 
            if (cardLogic.currentLocation != CardLogic.Location.Hand)
                continue;
            if (player != cardLogic.cardOwner)
            {
                cardLogic.FlipFaceDown();
                continue;
            }
            cardLogic.FlipFaceUp();
        }
    }

    public void EnableCardScrollScreen(List<CardLogic> cardLogics) =>
        UXManager.EnableCardScrollScreen(cardLogics);
    public void DisableCardScrollScreen() => UXManager.DisableCardScrollScreen();
}