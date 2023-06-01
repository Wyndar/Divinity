﻿using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
	public Game_Manager gm;
    public SaveManager SaveManager;
    public EnumConverter enumConverter;
    [SerializeField]
    private GameObject emptyHeroCardPrefab, emptySpellCardPrefab, emptyMonsterCardPrefab;

    //loads both deck and shield cards
    public List<CardLogic> LoadDeck(List<string> strings, List<Card> cards, GameObject deckObject, PlayerManager playerManager, bool isHeroDeck)
    {
        List<Card> database = new();
        database.AddRange(SaveManager.LoadCardDatabase(gm.DatabasePath));
        for (int i = 0; i < strings.Count; i++)
        {
            for (int j = 0; j < database.Count; j++)
            {
                if (strings[i] == database[j].Id)
                {
                    cards.Add(database[j]);
                }
            }
        }
        return CreateDeck(cards, deckObject, playerManager, isHeroDeck);
    }

    //parses instance of card (data) into instance of cardLogic (gameplay)
    public List<CardLogic> CreateDeck(List<Card> cards, GameObject deckObject, PlayerManager playerManager, bool isHeroDeck)
    {
        List<CardLogic> returnList = new();
        if (cards.Count >= 1)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                GameObject cardClone;
                CardLogic cardCloneCardLogic;
                GodLogic cardCloneGodLogic;

                //instantiates an empty incase art is missing
                switch (cards[i].CardType)
                {
                    case "spell":
                        cardClone = Instantiate(emptySpellCardPrefab);
                        cardCloneCardLogic = cardClone.AddComponent<SpellLogic>();
                        break;
                    case "monster":
                        cardClone = Instantiate(emptyMonsterCardPrefab);
                        cardCloneCardLogic = cardClone.AddComponent<MonsterLogic>();
                        break;
                    case "god":
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
                cardCloneCardLogic.id = cards[i].Id;
                cardCloneCardLogic.cardName = cards[i].CardName;
                cardClone.name = cardCloneCardLogic.cardName +" "+ playerManager.PlayerID + i.ToString();
                cardCloneCardLogic.cardFace = cardClone.transform.Find("Card Front");
                cardCloneCardLogic.cardBack = cardClone.transform.Find("Card Back");
                cardCloneCardLogic.cardImage = cardClone.transform.Find("Card Image");
                cardCloneCardLogic.cardOutline = cardClone.transform.Find("Card Outline");
                cardCloneCardLogic.GameManager = gm;
                cardCloneCardLogic.enumConverter = enumConverter;

                //attempts to change face card art, defaults missing art if error is encountered for whatever reason and sets a defaut

                    cardCloneCardLogic.image = Resources.Load("Sprites And Visuals/Card Images/" + cardCloneCardLogic.id, typeof(Sprite)) as Sprite;
                if(cardCloneCardLogic.image == null)
                {
                    cardCloneCardLogic.image = Resources.Load("Sprites And Visuals/Card Images/Default", typeof(Sprite)) as Sprite;
                }
                    cardCloneCardLogic.cardImage.gameObject.GetComponent<SpriteRenderer>().sprite = cardCloneCardLogic.image;

               
                //disables unnecessary components till needed
                cardCloneCardLogic.cardFace.gameObject.SetActive(false);
                cardCloneCardLogic.cardOutline.gameObject.SetActive(false);
                cardCloneCardLogic.cardType = cards[i].CardType;

                //populates generic data based on card type
                if (cardCloneCardLogic.cardType != "spell")
                {
                    cardCloneCombatantLogic = cardClone.AddComponent<CombatantLogic>();
                    cardCloneCombatantLogic.gm = gm;
                    cardCloneCombatantLogic.logic = cardCloneCardLogic;
                    cardCloneCombatantLogic.atk = cards[i].Atk;
                    cardCloneCombatantLogic.hp = cards[i].Hp;
                    cardCloneCombatantLogic.maxHp = cards[i].Hp;
                    cardCloneCombatantLogic.currentAtk = cards[i].Atk;
                    cardCloneCombatantLogic.currentHp = cards[i].Hp;
                    if (cardCloneCardLogic.cardType == "monster" && cards[i].MaxAttacks == 0)
                        cardCloneCombatantLogic.maxAttacks = 1;
                    else
                        cardCloneCombatantLogic.maxAttacks = cards[i].MaxAttacks;
                }
                if (cardCloneCardLogic.cardType != "god")
                {
                    cardClonePlayableLogic = cardClone.AddComponent<PlayableLogic>();
                    cardClonePlayableLogic.gm = gm;
                    cardClonePlayableLogic.logic = cardCloneCardLogic;
                    cardClonePlayableLogic.cost = cards[i].Cost;
                    if (isHeroDeck)
                        cardCloneCardLogic.currentLocation = CardLogic.Location.HeroDeck;
                    else
                        cardCloneCardLogic.currentLocation = CardLogic.Location.Deck;
                    cardCloneCardLogic.locationOrderNumber = i;
                    cardCloneCardLogic.transform.position = deckObject.transform.position;
                    cardCloneCardLogic.transform.rotation = deckObject.transform.rotation;
                    cardCloneCardLogic.isFaceDown = true;
                    cardCloneCardLogic.cardImage.gameObject.SetActive(false);
                    cardCloneCardLogic.cardBack.gameObject.SetActive(true);
                }
             
                //incase more card types are added eventually, switch allows easy implementation of additional logic
                switch (cardCloneCardLogic.cardType)
                {
                    case "spell":
                        cardClone.GetComponent<SpellLogic>().gm = gm;
                        break;
                    case "monster":
                        cardClone.GetComponent<MonsterLogic>().gm = gm;
                        cardClone.GetComponent<MonsterLogic>().combatLogic = cardClone.GetComponent<CombatantLogic>();
                        cardClone.GetComponent<MonsterLogic>().playLogic = cardClone.GetComponent<PlayableLogic>();
                        break;
                    case "god":
                        cardCloneCardLogic.currentLocation = CardLogic.Location.Field;
                        cardCloneCardLogic.locationOrderNumber = 99;
                        cardCloneCardLogic.transform.position = playerManager.hero.transform.position;
                        cardCloneCardLogic.transform.rotation = playerManager.hero.transform.rotation;
                        playerManager.heroCardLogic = cardClone.GetComponent<GodLogic>();
                        cardCloneCardLogic.isFaceDown = false;
                        cardCloneCardLogic.cardImage.gameObject.SetActive(true);
                        cardCloneCardLogic.cardBack.gameObject.SetActive(false);
                        cardCloneGodLogic = cardClone.GetComponent<GodLogic>();
                        cardCloneGodLogic.gm = gm;
                        cardCloneGodLogic.combatantLogic = cardClone.GetComponent<CombatantLogic>();
                        if (cardCloneGodLogic.maxShieldUsesPerTurn == 0)
                            cardCloneGodLogic.maxShieldUsesPerTurn = 1;
                        cardCloneGodLogic.shieldUsesLeft = cardCloneGodLogic.maxShieldUsesPerTurn;
                        break;
                }

                //populates more generic data
                cardCloneCardLogic.cardOwner = playerManager;
                cardCloneCardLogic.cardText = cards[i].CardText;
                cardCloneCardLogic.flavorText = cards[i].CardFlavorText;
                cardCloneCardLogic.effects = new List<Effect>(cards[i].Effects);
                if(cardCloneCardLogic.cardType != "god")
                    returnList.Add(cardCloneCardLogic);
            }
        }
        return returnList;
    }
}

