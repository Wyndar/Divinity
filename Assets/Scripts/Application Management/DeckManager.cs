using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DeckManager : GameManager
{
    [SerializeField] private ScrollingGodsPanelHandler godScrollView;
    [SerializeField] private ScrollingDecksPanelHandler deckScrollView;
    [SerializeField] private DeckEditManager deckEditManager;
    public Sprite lockSprite, unlockSprite;
    private SaveManager SaveManager;
    [SerializeField] private GameObject emptyHeroCardPrefab, emptySpellCardPrefab, emptyMonsterCardPrefab;
#pragma warning disable IDE0044 // Add readonly modifier
    private List<Card> database = new();
    private List<Card> godDatabase = new();
#pragma warning restore IDE0044 // Add readonly modifier
    private List<CardOwnedID> unlockedCardIDs = new();
    private List<CardOwnedID> unlockedGodIDs = new();
    private List<Deck> decks = new();

    private void Start()
    {
        SaveManager = GetComponent<SaveManager>();
        LoadDeck();
        DisplayGods();
        deckEditManager.InitializeDeckEdit();
    }

    //loads both deck and shield cards... for now
    public void LoadDeck()
    {
        if (database.Count == 0)
            database.AddRange(SaveManager.LoadCardDatabase("Load Data/Card Database/cardDatabase"));
        if (godDatabase.Count == 0)
            godDatabase.AddRange(SaveManager.LoadCardDatabase("Load Data/Card Database/divineDatabase"));
        try
        {
            unlockedCardIDs = SaveManager.LoadIDFromJson("Load Data/Player Data/unlockedCards");
            unlockedGodIDs = SaveManager.LoadIDFromJson("Load Data/Player Data/unlockedGods");
        }
        catch
        {
            unlockedCardIDs = SaveManager.ReadIDFromJson("Load Data/Starter Info/StarterCardsUnlockedIDs");
            unlockedGodIDs = SaveManager.ReadIDFromJson("Load Data/Starter Info/StarterGodsUnlockedIDs");
            SaveManager.SaveIDToJson("unlockedCards", unlockedCardIDs);
            SaveManager.SaveIDToJson("unlockedGods", unlockedGodIDs);
            Debug.Log("did not find unlocked cards, loaded starter cards");
        }
        try
        {
            decks = SaveManager.LoadDecksFromJson("decks");
        }
        catch
        {
            decks = SaveManager.ReadDecksFromJson("Load Data/Starter Info/decks");
            SaveManager.SaveDecksToJson("decks", decks);
            Debug.Log("did not find decks, loaded starter decks");
        }
    }

    private void DisplayGods()
    {
        List<Card> sendList = new();
        foreach (Card card in godDatabase)
        {
            if (card.Id.Any(x => char.IsDigit(x)) || !unlockedGodIDs.Any(x => x.ID == card.Id))
                continue;
            sendList.Add(card);
            //we need this for regular cards but not for gods
            //for (int i = 0; i < unlockedGodIDs.First(x => x.ID == card.Id).Count; i++)
            //    sendList.Add(card);
        }
        godScrollView.AddCardListToScrollCards(CreateCardScroll(sendList));
        godScrollView.AddGodCards(this);
    }
    public void DisplayDeckScroll (CardLogic godLogic)
    {
        deckScrollView.gameObject.SetActive(true);
        deckScrollView.RemoveContentDecks();
        deckScrollView.ClearScrollDecksList();
        foreach (Deck deck in decks)
            if(deck.GodID == godLogic.dataLogic.id)
                deckScrollView.AddDeckToScrollDecks(deck);
        deckScrollView.AddDecks(this);
    }
    public void OpenDeckEdit(Deck deck)
    {
        deckEditManager.gameObject.SetActive(true);
        GodLogic godLogic = (GodLogic)godScrollView.GetScrollCards().Find(x => x.dataLogic.id == deck.GodID);

        List<Card> sendList = new();
        foreach (Card card in godDatabase)
        {
            if (card.Id.Any(x => char.IsDigit(x)) && card.Id.Contains(godLogic.dataLogic.id))
            sendList.Add(card);
        }
        CardLogic[] powers = CreateCardScroll(sendList).ToArray();
        deckEditManager.SetDeck(godLogic, powers, deck);
    }
    public void CloseDeckEdit() => deckEditManager.gameObject.SetActive(false);
    public void UpdateDeck() => SaveManager.SaveDecksToJson("decks", decks);
    public void UpdateUnlockedCards() => SaveManager.SaveIDToJson("unlockedCards", unlockedCardIDs);
    public void UpdateUnlockedGods() => SaveManager.SaveIDToJson("unlockedGods", unlockedGodIDs);

    //parses instance of card (data) into instance of cardLogic (gameplay)
    public List<CardLogic> CreateCardScroll(List<Card> cards)
    {
        List<CardLogic> returnList = new();
        if (cards.Count == 0)
            return returnList;
        foreach (Card card in cards)
        {
            GameObject cardClone;
            CardLogic cardCloneCardLogic;
            GodLogic cardCloneGodLogic;

            //instantiates an empty incase art is missing
            switch (card.CardType)
            {
                case Type.Spell:
                    cardClone = Instantiate(emptySpellCardPrefab);
                    cardCloneCardLogic = cardClone.AddComponent<SpellLogic>();
                    break;
                case Type.Fighter:
                    cardClone = Instantiate(emptyMonsterCardPrefab);
                    cardCloneCardLogic = cardClone.AddComponent<MonsterLogic>();
                    break;
                case Type.God:
                    cardClone = Instantiate(emptyHeroCardPrefab);
                    cardCloneCardLogic = cardClone.AddComponent<GodLogic>();
                    break;
                default:
                    cardClone = Instantiate(emptyHeroCardPrefab);
                    cardCloneCardLogic = cardClone.AddComponent<CardLogic>();
                    break;
            }
            //adds additional components of the card logics to the card instance
            cardCloneCardLogic.dataLogic = cardClone.AddComponent<CardDataLogic>();
            cardCloneCardLogic.visualsLogic = cardClone.AddComponent<CardVisualsLogic>();
            cardCloneCardLogic.targetingLogic = cardClone.AddComponent<CardTargetingLogic>();
            cardCloneCardLogic.effectLogic = cardClone.AddComponent<CardEffectLogic>();
            cardCloneCardLogic.triggerLogic = cardClone.AddComponent<CardTriggerLogic>();

            //intitializes the cardlogic var of the components except effect
            cardCloneCardLogic.visualsLogic.Initialize();
            cardCloneCardLogic.targetingLogic.Initialize();
            cardCloneCardLogic.triggerLogic.Initialize();
            cardCloneCardLogic.dataLogic.Initialize();
            //activates and adds logic to empty then references the logic
            cardClone.SetActive(true);

            //populates instance with data
            cardCloneCardLogic.dataLogic.id = card.Id;
            cardCloneCardLogic.dataLogic.cardName = card.CardName;
            cardClone.name = $"{cardCloneCardLogic.dataLogic.cardName} {returnList.Count}";
            cardCloneCardLogic.dataLogic.type = card.CardType;
            cardCloneCardLogic.dataLogic.cardText = card.CardText;
            cardCloneCardLogic.dataLogic.flavorText = card.CardFlavorText;
            cardCloneCardLogic.dataLogic.traits = new(card.Traits);
            cardCloneCardLogic.dataLogic.attunements = new(card.Attunements);

            //attempts to change face card art, defaults missing art if error is encountered for whatever reason and sets a default
            cardCloneCardLogic.visualsLogic.image = Resources.Load($"Sprites And Visuals/Card Images/{cardCloneCardLogic.dataLogic.id}", typeof(Sprite)) as Sprite;
            if (cardCloneCardLogic.visualsLogic.image == null)
                cardCloneCardLogic.visualsLogic.image = Resources.Load("Sprites And Visuals/Card Images/Default", typeof(Sprite)) as Sprite;


            //populates generic data based on card type
            cardCloneCardLogic.visualsLogic.cardBack = cardClone.transform.Find("Card Back");
            cardCloneCardLogic.visualsLogic.cardImage = cardClone.transform.Find("Card Image");
            cardCloneCardLogic.visualsLogic.cardImageBorder = cardClone.transform.Find("Card Image Border");
            cardCloneCardLogic.visualsLogic.cardOutline = cardClone.transform.Find("Card Outline");
            cardCloneCardLogic.visualsLogic.textCanvas = cardClone.transform.Find("Canvas");

            //disables unnecessary components till needed
            cardCloneCardLogic.visualsLogic.cardOutline.gameObject.SetActive(false);
            cardCloneCardLogic.visualsLogic.cardImageBorder.gameObject.SetActive(false);
            cardCloneCardLogic.visualsLogic.textCanvas.gameObject.SetActive(false);
            cardCloneCardLogic.visualsLogic.cardImage.GetComponent<SpriteRenderer>().sprite = cardCloneCardLogic.visualsLogic.image;
            if (cardCloneCardLogic.dataLogic.type != Type.God)
            {
                PlayableLogic cardClonePlayableLogic = cardClone.AddComponent<PlayableLogic>();
                cardCloneCardLogic.dataLogic.playTypes.Add(PlayType.Playable);
                cardClonePlayableLogic.logic = cardCloneCardLogic;
                cardCloneCardLogic.visualsLogic.EnergyIcon = cardClone.transform.Find("Energy");
                cardCloneCardLogic.visualsLogic.costText = cardCloneCardLogic.visualsLogic.textCanvas.Find("Cost Text").GetComponent<TMP_Text>();
                cardClonePlayableLogic.cost = card.Cost;
                cardCloneCardLogic.visualsLogic.costText.text = cardClonePlayableLogic.cost.ToString();
            }
            cardCloneCardLogic.visualsLogic.isFaceDown = true;
            cardCloneCardLogic.visualsLogic.cardImage.gameObject.SetActive(false);
            cardCloneCardLogic.visualsLogic.cardBack.gameObject.SetActive(true);
            if (cardCloneCardLogic.dataLogic.type != Type.Spell)
            {
                CombatantLogic cardCloneCombatantLogic = cardClone.AddComponent<CombatantLogic>();
                cardCloneCardLogic.dataLogic.playTypes.Add(PlayType.Combatant);
                cardCloneCombatantLogic.logic = cardCloneCardLogic;
                cardCloneCombatantLogic.atk = card.Atk;
                cardCloneCombatantLogic.hp = card.Hp;
                cardCloneCombatantLogic.maxHp = card.Hp;
                cardCloneCombatantLogic.currentAtk = card.Atk;
                cardCloneCombatantLogic.currentHp = card.Hp;
            }
            //incase more card types are added eventually, switch allows easy implementation of additional logic
            switch (cardCloneCardLogic.dataLogic.type)
            {
                case Type.Spell:

                    break;
                case Type.Fighter:
                    cardClone.GetComponent<MonsterLogic>().combatLogic = cardClone.GetComponent<CombatantLogic>();
                    cardClone.GetComponent<MonsterLogic>().playLogic = cardClone.GetComponent<PlayableLogic>();
                    cardCloneCardLogic.visualsLogic.ATKText = cardCloneCardLogic.visualsLogic.textCanvas.Find("ATK Text").GetComponent<TMP_Text>();
                    cardCloneCardLogic.visualsLogic.HPText = cardCloneCardLogic.visualsLogic.textCanvas.Find("HP Text").GetComponent<TMP_Text>();
                    cardCloneCardLogic.visualsLogic.ATKText.text = cardClone.GetComponent<CombatantLogic>().atk.ToString();
                    cardCloneCardLogic.visualsLogic.HPText.text = cardClone.GetComponent<CombatantLogic>().hp.ToString();
                    cardClone.GetComponent<CombatantLogic>().maxAttacks = card.MaxAttacks == 0 ? 1 : card.MaxAttacks;
                    break;
                case Type.God:
                    cardCloneGodLogic = cardClone.GetComponent<GodLogic>();
                    cardCloneGodLogic.combatantLogic = cardClone.GetComponent<CombatantLogic>();
                    if (cardCloneGodLogic.maxShieldUsesPerTurn == 0)
                        cardCloneGodLogic.maxShieldUsesPerTurn = 1;
                    cardCloneGodLogic.shieldUsesLeft = cardCloneGodLogic.maxShieldUsesPerTurn;
                    cardCloneGodLogic.attunementRates = new(card.AttunementRates);
                    break;
            }
            cardCloneCardLogic.effectLogic.Initialize();
            cardCloneCardLogic.effectLogic.effects = new();
            cardCloneCardLogic.effectLogic.effects = new();
            foreach (Effect effect in card.Effects)
            {
                Effect effectInstance = new(effect);
                cardCloneCardLogic.effectLogic.effects.Add(effectInstance);
            }
            returnList.Add(cardCloneCardLogic);
            cardClone.SetActive(false);
        }
        return returnList;
    }
}
