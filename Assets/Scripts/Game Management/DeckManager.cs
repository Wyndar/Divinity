//NB: null checks are not redundant
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private Game_Manager G_M;
    [SerializeField] private SaveManager SaveManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager U_I;
    [SerializeField] private GameObject emptyHeroCardPrefab, emptySpellCardPrefab, emptyMonsterCardPrefab;
    [SerializeField] private List<Card> database = new();
    [SerializeField] private List<Card> godDatabase = new();

    //loads both deck and shield cards... for now
    public List<CardLogic> LoadDeck(List<string> strings, List<Card> cards, GameObject deckObject, PlayerManager playerManager, bool isHeroDeck)
    {
        if (!isHeroDeck)
        {
            if (database.Count == 0)
                database.AddRange(SaveManager.LoadCardDatabase(G_M.CardDatabasePath));
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
                godDatabase.AddRange(SaveManager.LoadCardDatabase(G_M.GodDatabasePath));
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
                PlayableLogic cardClonePlayableLogic;
                CombatantLogic cardCloneCombatantLogic;

                //populates instance with data
                cardCloneCardLogic.id = card.Id;
                cardCloneCardLogic.cardName = card.CardName;
                cardClone.name = $"{playerManager.PlayerName}'s {cardCloneCardLogic.cardName} {returnList.Count}";
                cardCloneCardLogic.cardFace = cardClone.transform.Find("Card Front");
                cardCloneCardLogic.cardBack = cardClone.transform.Find("Card Back");
                cardCloneCardLogic.cardImage = cardClone.transform.Find("Card Image");
                cardCloneCardLogic.cardOutline = cardClone.transform.Find("Card Outline");
                cardCloneCardLogic.gameManager = G_M;
                cardCloneCardLogic.audioManager = audioManager;

                //attempts to change face card art, defaults missing art if error is encountered for whatever reason and sets a default
                cardCloneCardLogic.image = Resources.Load($"Sprites And Visuals/Card Images/{cardCloneCardLogic.id}", typeof(Sprite)) as Sprite;
                if(cardCloneCardLogic.image == null)
                    cardCloneCardLogic.image = Resources.Load("Sprites And Visuals/Card Images/Default", typeof(Sprite)) as Sprite;

                cardCloneCardLogic.cardImage.gameObject.GetComponent<SpriteRenderer>().sprite = cardCloneCardLogic.image;
               
                //disables unnecessary components till needed
                cardCloneCardLogic.cardFace.gameObject.SetActive(false);
                cardCloneCardLogic.cardOutline.gameObject.SetActive(false);
                cardCloneCardLogic.type = card.CardType;

                //populates generic data based on card type
                if (cardCloneCardLogic.type != Type.Spell)
                {
                    cardCloneCombatantLogic = cardClone.AddComponent<CombatantLogic>();
                    cardCloneCombatantLogic.gm = G_M;
                    cardCloneCombatantLogic.logic = cardCloneCardLogic;
                    cardCloneCombatantLogic.atk = card.Atk;
                    cardCloneCombatantLogic.hp = card.Hp;
                    cardCloneCombatantLogic.maxHp = card.Hp;
                    cardCloneCombatantLogic.currentAtk = card.Atk;
                    cardCloneCombatantLogic.currentHp = card.Hp;
                    if (cardCloneCardLogic.type == Type.Fighter && card.MaxAttacks == 0)
                        cardCloneCombatantLogic.maxAttacks = 1;
                    else
                        cardCloneCombatantLogic.maxAttacks = card.MaxAttacks;
                }
                if (cardCloneCardLogic.type != Type.God)
                {
                    cardClonePlayableLogic = cardClone.AddComponent<PlayableLogic>();
                    cardClonePlayableLogic.gm = G_M;
                    cardClonePlayableLogic.logic = cardCloneCardLogic;
                    cardClonePlayableLogic.cost = card.Cost;
                    if (isHeroDeck)
                        cardCloneCardLogic.currentLocation = Location.HeroDeck;
                    else
                        cardCloneCardLogic.currentLocation = Location.Deck;

                    cardCloneCardLogic.transform.SetPositionAndRotation(new Vector3(deckObject.transform.position.x,
                        deckObject.transform.position.y, deckObject.transform.position.z + 0.5f), deckObject.transform.rotation);
                    cardCloneCardLogic.isFaceDown = true;
                    cardCloneCardLogic.cardImage.gameObject.SetActive(false);
                    cardCloneCardLogic.cardBack.gameObject.SetActive(true);
                }
             
                //incase more card types are added eventually, switch allows easy implementation of additional logic
                switch (cardCloneCardLogic.type)
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
                        break;
                    case Type.God:
                        cardCloneCardLogic.currentLocation = Location.Field;
                        cardCloneCardLogic.locationOrderNumber = 99;
                        cardCloneCardLogic.transform.position = playerManager.hero.transform.position;
                        cardCloneCardLogic.transform.rotation = playerManager.hero.transform.rotation;
                        playerManager.heroCardLogic = cardClone.GetComponent<GodLogic>();
                        cardCloneCardLogic.isFaceDown = false;
                        cardCloneCardLogic.cardImage.gameObject.SetActive(true);
                        cardCloneCardLogic.cardBack.gameObject.SetActive(false);
                        cardCloneGodLogic = cardClone.GetComponent<GodLogic>();
                        cardCloneGodLogic.gm = G_M;
                        cardClone.GetComponent<GodLogic>().U_I = U_I;
                        cardCloneGodLogic.combatantLogic = cardClone.GetComponent<CombatantLogic>();
                        if (cardCloneGodLogic.maxShieldUsesPerTurn == 0)
                            cardCloneGodLogic.maxShieldUsesPerTurn = 1;
                        cardCloneGodLogic.shieldUsesLeft = cardCloneGodLogic.maxShieldUsesPerTurn;
                        break;
                }

                //populates more generic data
                cardCloneCardLogic.cardOwner = playerManager;
                cardCloneCardLogic.cardController = playerManager;
                cardCloneCardLogic.cardText = card.CardText;
                cardCloneCardLogic.flavorText = card.CardFlavorText;
                cardCloneCardLogic.effects = new();
                if (card.Traits != null)
                    cardCloneCardLogic.traits = new(card.Traits);
                foreach (Effect effect in card.Effects)
                {
                    effect.maxActivations = effect.MaxActivations;
                    effect.duration = effect.Duration;
                    effect.triggerCardOwner = effect.TriggerController;
                    effect.triggerCardOwner = effect.TriggerController;

                    if (effect.ActivationLocations != null)
                        effect.activationLocations = new(effect.ActivationLocations);
                    if (effect.TriggerLocations != null)
                        effect.triggerLocations = new(effect.TriggerLocations);
                    if (effect.TriggerStates != null)
                        effect.triggerStates = new(effect.TriggerStates);
                    if (effect.TriggerPhases != null)
                        effect.triggerPhases = new(effect.TriggerPhases);
                    if (effect.TriggerCardLocations != null)
                        effect.triggerCardLocations = new(effect.TriggerCardLocations);
                    if (effect.TriggerEffects != null)
                        effect.triggerEffects = new(effect.TriggerEffects);

                    foreach (SubEffect subEffect in effect.SubEffects)
                    {
                        subEffect.parentEffect = effect;
                        subEffect.effectAmount = subEffect.EffectAmount;
                        subEffect.effectTargetAmount = subEffect.EffectTargetAmount;
                        subEffect.effectUsed = subEffect.EffectUsed;
                        subEffect.effectType = subEffect.EffectType;
                        subEffect.effectTargetController = subEffect.EffectTargetController;
                        subEffect.targetingType = subEffect.TargetingType;
                        if (subEffect.TargetLocations != null)
                            subEffect.targetLocations = new(subEffect.TargetLocations);
                        if (subEffect.EffectTargetTypes != null)
                            subEffect.effectTargetTypes = new(subEffect.EffectTargetTypes);
                    }
                    cardCloneCardLogic.effects.Add(effect);
                }

                if (cardCloneCardLogic.type != Type.God)
                    returnList.Add(cardCloneCardLogic);
            }
        }
        return returnList;
    }
}

