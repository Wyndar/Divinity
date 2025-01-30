using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField] public ScrollingDeckPanelHandler godScrollView, cardScrollView;
    private SaveManager SaveManager;
    [SerializeField] private GameObject emptyHeroCardPrefab, emptySpellCardPrefab, emptyMonsterCardPrefab;
    private List<Card> database = new();
    private List<Card> godDatabase = new();
    private List<string> decks = new();

    private void Start()
    {
        SaveManager = GetComponent<SaveManager>();
        LoadDeck();
        DisplayGods();
    }

    //loads both deck and shield cards... for now
    public void LoadDeck()
    {
        if (database.Count == 0)
            database.AddRange(SaveManager.LoadCardDatabase("Load Data/Card Database/cardDatabase"));
        if (godDatabase.Count == 0)
            godDatabase.AddRange(SaveManager.LoadCardDatabase("Load Data/Card Database/divineDatabase"));
        string json;
        try
        {
            json = File.ReadAllText(Application.persistentDataPath + "decks");
        }
        catch
        {
            return;
        }
        decks = JsonConvert.DeserializeObject<List<string>>(json, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
    }

    private void DisplayGods()
    {
        List<Card> sendList = new();
        foreach (Card card in godDatabase)
        {
            if (card.Id.Any(x => char.IsDigit(x)))
                continue;
            sendList.Add(card);
        }
        godScrollView.AddCardListToScrollCards(CreateCardScroll(sendList));
        godScrollView.AddContentCards();
    }


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

            //activates and adds logic to empty then references the logic
            cardClone.SetActive(true);

            //populates instance with data
            cardCloneCardLogic.id = card.Id;
            cardCloneCardLogic.cardName = card.CardName;
            cardClone.name = $"{cardCloneCardLogic.cardName} {returnList.Count}";
            cardCloneCardLogic.type = card.CardType;
            cardCloneCardLogic.cardText = card.CardText;
            cardCloneCardLogic.flavorText = card.CardFlavorText;
            cardCloneCardLogic.traits = new(card.Traits);
            cardCloneCardLogic.attunements = new(card.Attunements);

            //attempts to change face card art, defaults missing art if error is encountered for whatever reason and sets a default
            cardCloneCardLogic.image = Resources.Load($"Sprites And Visuals/Card Images/{cardCloneCardLogic.id}", typeof(Sprite)) as Sprite;
            if (cardCloneCardLogic.image == null)
                cardCloneCardLogic.image = Resources.Load("Sprites And Visuals/Card Images/Default", typeof(Sprite)) as Sprite;


            //populates generic data based on card type
            cardCloneCardLogic.cardBack = cardClone.transform.Find("Card Back");
            cardCloneCardLogic.cardImage = cardClone.transform.Find("Card Image");
            cardCloneCardLogic.cardImageBorder = cardClone.transform.Find("Card Image Border");
            cardCloneCardLogic.cardOutline = cardClone.transform.Find("Card Outline");
            cardCloneCardLogic.textCanvas = cardClone.transform.Find("Canvas");

            //disables unnecessary components till needed
            cardCloneCardLogic.cardOutline.gameObject.SetActive(false);
            cardCloneCardLogic.cardImageBorder.gameObject.SetActive(false);
            cardCloneCardLogic.textCanvas.gameObject.SetActive(false);
            cardCloneCardLogic.cardImage.GetComponent<SpriteRenderer>().sprite = cardCloneCardLogic.image;
            if (cardCloneCardLogic.type != Type.God)
            {
                PlayableLogic cardClonePlayableLogic = cardClone.AddComponent<PlayableLogic>();
                cardCloneCardLogic.playTypes.Add(PlayType.Playable);
                cardClonePlayableLogic.logic = cardCloneCardLogic;
                cardCloneCardLogic.EnergyIcon = cardClone.transform.Find("Energy");
                cardCloneCardLogic.costText = cardCloneCardLogic.textCanvas.Find("Cost Text").GetComponent<TMP_Text>();
                cardClonePlayableLogic.cost = card.Cost;
                cardCloneCardLogic.costText.text = cardClonePlayableLogic.cost.ToString();
            }
            cardCloneCardLogic.isFaceDown = true;
            cardCloneCardLogic.cardImage.gameObject.SetActive(false);
            cardCloneCardLogic.cardBack.gameObject.SetActive(true);
            if (cardCloneCardLogic.type != Type.Spell)
            {
                CombatantLogic cardCloneCombatantLogic = cardClone.AddComponent<CombatantLogic>();
                cardCloneCardLogic.playTypes.Add(PlayType.Combatant);
                cardCloneCombatantLogic.logic = cardCloneCardLogic;
                cardCloneCombatantLogic.atk = card.Atk;
                cardCloneCombatantLogic.hp = card.Hp;
                cardCloneCombatantLogic.maxHp = card.Hp;
                cardCloneCombatantLogic.currentAtk = card.Atk;
                cardCloneCombatantLogic.currentHp = card.Hp;
            }
            //incase more card types are added eventually, switch allows easy implementation of additional logic
            switch (cardCloneCardLogic.type)
            {
                case Type.Spell:

                    break;
                case Type.Fighter:
                    cardClone.GetComponent<MonsterLogic>().combatLogic = cardClone.GetComponent<CombatantLogic>();
                    cardClone.GetComponent<MonsterLogic>().playLogic = cardClone.GetComponent<PlayableLogic>();
                    cardCloneCardLogic.ATKText = cardCloneCardLogic.textCanvas.Find("ATK Text").GetComponent<TMP_Text>();
                    cardCloneCardLogic.HPText = cardCloneCardLogic.textCanvas.Find("HP Text").GetComponent<TMP_Text>();
                    cardCloneCardLogic.ATKText.text = cardClone.GetComponent<CombatantLogic>().atk.ToString();
                    cardCloneCardLogic.HPText.text = cardClone.GetComponent<CombatantLogic>().hp.ToString();
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

            cardCloneCardLogic.effects = new();
            foreach (Effect effect in card.Effects)
            {
                Effect effectInstance = new(effect);
                cardCloneCardLogic.effects.Add(effectInstance);
            }
            returnList.Add(cardCloneCardLogic);
        }
        return returnList;
    }
}
