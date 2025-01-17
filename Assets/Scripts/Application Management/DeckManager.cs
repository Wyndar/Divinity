using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DeckManager : MonoBehaviour
{
    [SerializeField] public ScrollView godScrollView, cardScrollView;
    private SaveManager SaveManager;
    [SerializeField] private GameObject emptyHeroCardPrefab, emptySpellCardPrefab, emptyMonsterCardPrefab;
    private List<Card> database = new();
    private List<Card> godDatabase = new();

    private void Start()
    {
        SaveManager = GetComponent<SaveManager>();
        LoadDeck();
    }

    //loads both deck and shield cards... for now
    public void LoadDeck()
    {
        if (database.Count == 0)
            database.AddRange(SaveManager.LoadCardDatabase("Load Data/Card Database/cardDatabase"));
        if (godDatabase.Count == 0)
            godDatabase.AddRange(SaveManager.LoadCardDatabase("Load Data/Card Database/divineDatabase"));
    }

    //parses instance of card (data) into instance of cardLogic (gameplay)
    public List<CardLogic> CreateDeck(List<Card> cards, GameObject deckObject, PlayerManager playerManager, bool isHeroDeck)
    {
        List<CardLogic> returnList = new();
        if (cards.Count > 0)
        {
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
                        cardClone = playerManager.hero;
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
                cardClone.name = $"{playerManager.PlayerName}'s {cardCloneCardLogic.cardName} {returnList.Count}";
                cardCloneCardLogic.type = card.CardType;
                cardCloneCardLogic.cardOwner = playerManager;
                cardCloneCardLogic.cardController = playerManager;
                cardCloneCardLogic.cardText = card.CardText;
                cardCloneCardLogic.flavorText = card.CardFlavorText;
                cardCloneCardLogic.traits = new(card.Traits);
                cardCloneCardLogic.attunements = new(card.Attunements);

                //attempts to change face card art, defaults missing art if error is encountered for whatever reason and sets a default
                cardCloneCardLogic.image = Resources.Load($"Sprites And Visuals/Card Images/{cardCloneCardLogic.id}", typeof(Sprite)) as Sprite;
                if (cardCloneCardLogic.image == null)
                    cardCloneCardLogic.image = Resources.Load("Sprites And Visuals/Card Images/Default", typeof(Sprite)) as Sprite;


                //populates generic data based on card type
                if (cardCloneCardLogic.type != Type.God)
                {
                    cardCloneCardLogic.cardBack = cardClone.transform.Find("Card Back");
                    cardCloneCardLogic.cardImage = cardClone.transform.Find("Card Image");
                    cardCloneCardLogic.cardImageBorder = cardClone.transform.Find("Card Image Border");
                    cardCloneCardLogic.cardOutline = cardClone.transform.Find("Card Outline");
                    cardCloneCardLogic.textCanvas = cardClone.transform.Find("Canvas");
                    cardCloneCardLogic.EnergyIcon = cardClone.transform.Find("Energy");
                    cardCloneCardLogic.costText = cardCloneCardLogic.textCanvas.Find("Cost Text").GetComponent<TMP_Text>();

                    //disables unnecessary components till needed
                    cardCloneCardLogic.cardOutline.gameObject.SetActive(false);
                    cardCloneCardLogic.cardImageBorder.gameObject.SetActive(false);
                    cardCloneCardLogic.textCanvas.gameObject.SetActive(false);

                    cardCloneCardLogic.cardImage.GetComponent<SpriteRenderer>().sprite = cardCloneCardLogic.image;
                    PlayableLogic cardClonePlayableLogic = cardClone.AddComponent<PlayableLogic>();
                    cardCloneCardLogic.playTypes.Add(PlayType.Playable);
                    cardClonePlayableLogic.logic = cardCloneCardLogic;
                    cardClonePlayableLogic.cost = card.Cost;
                    cardCloneCardLogic.costText.text = cardClonePlayableLogic.cost.ToString();
                    if (isHeroDeck)
                        cardCloneCardLogic.currentLocation = Location.HeroDeck;
                    else
                        cardCloneCardLogic.currentLocation = Location.Deck;

                    cardCloneCardLogic.transform.SetPositionAndRotation(new Vector3(deckObject.transform.position.x - 0.5f,
                        deckObject.transform.position.y, deckObject.transform.position.z),
                        deckObject.transform.rotation);
                    cardCloneCardLogic.isFaceDown = true;
                    cardCloneCardLogic.cardImage.gameObject.SetActive(false);
                    cardCloneCardLogic.cardBack.gameObject.SetActive(true);
                }
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
                        cardCloneCardLogic.currentLocation = Location.Field;
                        cardCloneCardLogic.locationOrderNumber = 99;
                        playerManager.heroCardLogic = cardClone.GetComponent<GodLogic>();
                        cardCloneCardLogic.isFaceDown = false;
                        cardClone.GetComponent<SpriteRenderer>().sprite = cardCloneCardLogic.image;
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

                if (cardCloneCardLogic.type != Type.God)
                    returnList.Add(cardCloneCardLogic);
            }
        }
        return returnList;
    }
}
