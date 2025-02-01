//NB: null checks are not redundant
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeckLoaderManager : MonoBehaviour
{
    [SerializeField] private GameBattleManager G_M;
    [SerializeField] private SaveManager SaveManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SecondaryUIManager U_I;
    [SerializeField] private GameObject emptyHeroCardPrefab, emptySpellCardPrefab, emptyMonsterCardPrefab;
    private List<Card> database = new();
    private List<Card> godDatabase = new();

    //loads both deck and shield cards... for now
    public List<CardLogic> LoadDeck(List<string> strings, List<Card> cards, GameObject deckObject, PlayerManager playerManager, bool isHeroDeck)
    {
        if (!isHeroDeck)
        {
            if (database.Count == 0)
                database.AddRange(SaveManager.LoadCardDatabase("Load Data/Card Database/cardDatabase"));
            foreach (string cardID in strings)
            {
                //try faster direct lookup first
                try
                {
                    Card c = database[int.Parse(cardID.Split("-")[1]) - 1];
                    if (c.Id == cardID)
                    {
                        cards.Add(c);
                        continue;
                    }
                }
                catch
                {
                    foreach (Card card in database)
                        if (card.Id == cardID)
                        {
                            cards.Add(card);
                            break;
                        }
                    continue;
                }
            }
        }
        else
        {
            if (godDatabase.Count == 0)
                godDatabase.AddRange(SaveManager.LoadCardDatabase("Load Data/Card Database/divineDatabase"));
            foreach (string cardID in strings)
                foreach (Card card in godDatabase)
                    if (cardID == card.Id)
                    {
                        cards.Add(card);
                        break;
                    }
        }
        return CreateDeck(cards, deckObject, playerManager, isHeroDeck);
    }

    //parses instance of card (data) into instance of cardLogic (gameplay)
    public List<CardLogic> CreateDeck(List<Card> cards, GameObject deckObject, PlayerManager playerManager, bool isHeroDeck)
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
                    cardClone = playerManager.hero;
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

            //intitializes the cardlogic var of the components
            cardCloneCardLogic.visualsLogic.Initialize();
            cardCloneCardLogic.effectLogic.Initialize();
            cardCloneCardLogic.targetingLogic.Initialize();
            cardCloneCardLogic.triggerLogic.Initialize();
            cardCloneCardLogic.dataLogic.Initialize();

            //activates and adds logic to empty then references the logic
            cardClone.SetActive(true);

            //populates instance with data
            cardCloneCardLogic.dataLogic.id = card.Id;
            cardCloneCardLogic.dataLogic.cardName = card.CardName;
            cardClone.name = $"{playerManager.PlayerName}'s {cardCloneCardLogic.dataLogic.cardName} {returnList.Count}";
            cardCloneCardLogic.dataLogic.type = card.CardType;
            cardCloneCardLogic.dataLogic.cardOwner = playerManager;
            cardCloneCardLogic.dataLogic.cardController = playerManager;
            cardCloneCardLogic.dataLogic.cardText = card.CardText;
            cardCloneCardLogic.dataLogic.flavorText = card.CardFlavorText;
            cardCloneCardLogic.dataLogic.traits = new(card.Traits);
            cardCloneCardLogic.dataLogic.attunements = new(card.Attunements);

            cardCloneCardLogic.gameManager = G_M;
            cardCloneCardLogic.audioManager = audioManager;

            //attempts to change face card art, defaults missing art if error is encountered for whatever reason and sets a default
            cardCloneCardLogic.visualsLogic.image = Resources.Load($"Sprites And Visuals/Card Images/{cardCloneCardLogic.dataLogic.id}", typeof(Sprite)) as Sprite;
            if (cardCloneCardLogic.visualsLogic.image == null)
                cardCloneCardLogic.visualsLogic.image = Resources.Load("Sprites And Visuals/Card Images/Default", typeof(Sprite)) as Sprite;


            //populates generic data based on card type
            if (cardCloneCardLogic.dataLogic.type != Type.God)
            {
                cardCloneCardLogic.visualsLogic.cardBack = cardClone.transform.Find("Card Back");
                cardCloneCardLogic.visualsLogic.cardImage = cardClone.transform.Find("Card Image");
                cardCloneCardLogic.visualsLogic.cardImageBorder = cardClone.transform.Find("Card Image Border");
                cardCloneCardLogic.visualsLogic.cardOutline = cardClone.transform.Find("Card Outline");
                cardCloneCardLogic.visualsLogic.textCanvas = cardClone.transform.Find("Canvas");
                cardCloneCardLogic.visualsLogic.EnergyIcon = cardClone.transform.Find("Energy");
                cardCloneCardLogic.visualsLogic.costText = cardCloneCardLogic.visualsLogic.textCanvas.Find("Cost Text").GetComponent<TMP_Text>();

                //disables unnecessary components till needed
                cardCloneCardLogic.visualsLogic.cardOutline.gameObject.SetActive(false);
                cardCloneCardLogic.visualsLogic.cardImageBorder.gameObject.SetActive(false);
                cardCloneCardLogic.visualsLogic.textCanvas.gameObject.SetActive(false);

                cardCloneCardLogic.visualsLogic.cardImage.GetComponent<SpriteRenderer>().sprite = cardCloneCardLogic.visualsLogic.image;
                PlayableLogic cardClonePlayableLogic = cardClone.AddComponent<PlayableLogic>();
                cardCloneCardLogic.dataLogic.playTypes.Add(PlayType.Playable);
                cardClonePlayableLogic.gm = G_M;
                cardClonePlayableLogic.logic = cardCloneCardLogic;
                cardClonePlayableLogic.cost = card.Cost;
                cardCloneCardLogic.visualsLogic.costText.text = cardClonePlayableLogic.cost.ToString();
                if (isHeroDeck)
                    cardCloneCardLogic.dataLogic.currentLocation = Location.HeroDeck;
                else
                    cardCloneCardLogic.dataLogic.currentLocation = Location.Deck;

                cardCloneCardLogic.dataLogic.transform.SetPositionAndRotation(new Vector3(deckObject.transform.position.x - 0.5f,
                    deckObject.transform.position.y, deckObject.transform.position.z),
                    deckObject.transform.rotation);
                cardCloneCardLogic.visualsLogic.isFaceDown = true;
                cardCloneCardLogic.visualsLogic.cardImage.gameObject.SetActive(false);
                cardCloneCardLogic.visualsLogic.cardBack.gameObject.SetActive(true);
            }
            if (cardCloneCardLogic.dataLogic.type != Type.Spell)
            {
                CombatantLogic cardCloneCombatantLogic = cardClone.AddComponent<CombatantLogic>();
                cardCloneCardLogic.dataLogic.playTypes.Add(PlayType.Combatant);
                cardCloneCombatantLogic.gm = G_M;
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
                    cardClone.GetComponent<SpellLogic>().gm = G_M;
                    cardClone.GetComponent<SpellLogic>().U_I = U_I;
                    break;
                case Type.Fighter:
                    cardClone.GetComponent<MonsterLogic>().gm = G_M;
                    cardClone.GetComponent<MonsterLogic>().U_I = U_I;
                    cardClone.GetComponent<MonsterLogic>().combatLogic = cardClone.GetComponent<CombatantLogic>();
                    cardClone.GetComponent<MonsterLogic>().playLogic = cardClone.GetComponent<PlayableLogic>();
                    cardCloneCardLogic.visualsLogic.ATKText = cardCloneCardLogic.visualsLogic.textCanvas.Find("ATK Text").GetComponent<TMP_Text>();
                    cardCloneCardLogic.visualsLogic.HPText = cardCloneCardLogic.visualsLogic.textCanvas.Find("HP Text").GetComponent<TMP_Text>();
                    cardCloneCardLogic.visualsLogic.ATKText.text = cardClone.GetComponent<CombatantLogic>().atk.ToString();
                    cardCloneCardLogic.visualsLogic.HPText.text = cardClone.GetComponent<CombatantLogic>().hp.ToString();
                    cardClone.GetComponent<CombatantLogic>().maxAttacks = card.MaxAttacks == 0 ? 1 : card.MaxAttacks;
                    break;
                case Type.God:
                    cardCloneCardLogic.dataLogic.currentLocation = Location.Field;
                    cardCloneCardLogic.dataLogic.locationOrderNumber = 99;
                    playerManager.heroCardLogic = cardClone.GetComponent<GodLogic>();
                    cardCloneCardLogic.visualsLogic.isFaceDown = false;
                    cardClone.GetComponent<SpriteRenderer>().sprite = cardCloneCardLogic.visualsLogic.image;
                    cardCloneGodLogic = cardClone.GetComponent<GodLogic>();
                    cardCloneGodLogic.gm = G_M;
                    cardClone.GetComponent<GodLogic>().U_I = U_I;
                    cardCloneGodLogic.combatantLogic = cardClone.GetComponent<CombatantLogic>();
                    if (cardCloneGodLogic.maxShieldUsesPerTurn == 0)
                        cardCloneGodLogic.maxShieldUsesPerTurn = 1;
                    cardCloneGodLogic.shieldUsesLeft = cardCloneGodLogic.maxShieldUsesPerTurn;
                    cardCloneGodLogic.attunementRates = new(card.AttunementRates);
                    cardClone.GetComponent<CombatantLogic>().targetState = TargetState.Default;
                    G_M.OnPhaseChange += cardCloneCardLogic.GetPhaseTriggers;
                    G_M.OnEffectTrigger += cardCloneCardLogic.GetEffectTriggers;
                    G_M.OnStateChange += cardCloneCardLogic.GetStateTriggers;
                    break;
            }

            cardCloneCardLogic.effectLogic.effects = new();
            foreach (Effect effect in card.Effects)
            {
                Effect effectInstance = new(effect);
                cardCloneCardLogic.effectLogic.effects.Add(effectInstance);
            }

            if (cardCloneCardLogic.dataLogic.type != Type.God)
                returnList.Add(cardCloneCardLogic);
        }
        return returnList;
    }
}

