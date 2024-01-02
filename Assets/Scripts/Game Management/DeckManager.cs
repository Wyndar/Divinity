using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
	public Game_Manager G_M;
    public SaveManager SaveManager;
    public EnumManager enumConverter;
    public AudioManager audioManager;
    public UIManager U_I;
    [SerializeField]
    private GameObject emptyHeroCardPrefab, emptySpellCardPrefab, emptyMonsterCardPrefab;

    //loads both deck and shield cards
    public List<CardLogic> LoadDeck(List<string> strings, List<Card> cards, GameObject deckObject, PlayerManager playerManager, bool isHeroDeck)
    {
        List<Card> database = new();
        database.AddRange(SaveManager.LoadCardDatabase(G_M.CardDatabasePath));
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
                cardCloneCardLogic.gameManager = G_M;
                cardCloneCardLogic.enumConverter = enumConverter;
                cardCloneCardLogic.audioManager = audioManager;

                //attempts to change face card art, defaults missing art if error is encountered for whatever reason and sets a defaut

                    cardCloneCardLogic.image = Resources.Load($"Sprites And Visuals/Card Images/{cardCloneCardLogic.id}", typeof(Sprite)) as Sprite;
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
                    cardCloneCombatantLogic.gm = G_M;
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
                    cardClonePlayableLogic.gm = G_M;
                    cardClonePlayableLogic.logic = cardCloneCardLogic;
                    cardClonePlayableLogic.cost = cards[i].Cost;
                    if (isHeroDeck)
                        cardCloneCardLogic.currentLocation = Location.HeroDeck;
                    else
                        cardCloneCardLogic.currentLocation = Location.Deck;

                    cardCloneCardLogic.locationOrderNumber = i;
                    cardCloneCardLogic.transform.SetPositionAndRotation(new Vector3(deckObject.transform.position.x,deckObject.transform.position.y,deckObject.transform.position.z+0.5f), deckObject.transform.rotation);
                    cardCloneCardLogic.isFaceDown = true;
                    cardCloneCardLogic.cardImage.gameObject.SetActive(false);
                    cardCloneCardLogic.cardBack.gameObject.SetActive(true);
                }
             
                //incase more card types are added eventually, switch allows easy implementation of additional logic
                switch (cardCloneCardLogic.cardType)
                {
                    case "spell":
                        cardClone.GetComponent<SpellLogic>().gm = G_M;
                        cardClone.GetComponent<SpellLogic>().U_I = U_I;
                        break;
                    case "monster":
                        cardClone.GetComponent<MonsterLogic>().gm = G_M;
                        cardClone.GetComponent<MonsterLogic>().U_I = U_I;
                        cardClone.GetComponent<MonsterLogic>().combatLogic = cardClone.GetComponent<CombatantLogic>();
                        cardClone.GetComponent<MonsterLogic>().playLogic = cardClone.GetComponent<PlayableLogic>();
                        break;
                    case "god":
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
                cardCloneCardLogic.cardText = cards[i].CardText;
                cardCloneCardLogic.flavorText = cards[i].CardFlavorText;
                cardCloneCardLogic.effects = new();
                cardCloneCardLogic.traits = new();
                foreach (string trait in cards[i].Traits)
                    cardCloneCardLogic.traits.Add(enumConverter.TraitStringToEnum(trait));
                foreach(Effect effect in cards[i].Effects)
                {
                    Effect newEffect = new CopyEffect(effect); cardCloneCardLogic.effects.Add(newEffect);
                }
                foreach (Effect effect in cardCloneCardLogic.effects)
                {
                    effect.effectTypes = new();
                    effect.triggerTypes = new();
                    effect.effectsUsed = new();
                    effect.activationLocation = new();
                    effect.triggerLocation = new();
                    effect.targetLocation = new();
                    effect.triggerState = new();
                    effect.triggerPhase = new();
                    effect.triggerCardLocation = new();
                    effect.triggerCardOwner = new();
                    effect.maxActivations = effect.MaxActivations;
                    effect.effectTargetController = new();
                    effect.targetingTypes = new();
                    effect.effectTargetPlayType = new();
                    effect.effectTargetType = new();
                    foreach (string effectUsed in effect.EffectUsed)
                        effect.effectsUsed.Add(enumConverter.EffectUsedStringToEnum(effectUsed));
                    if (effect.TriggerType != null)
                        foreach (string triggerType in effect.TriggerType)
                            effect.triggerTypes.Add(enumConverter.EffectUsedStringToEnum(triggerType));
                    foreach (string effectType in effect.EffectType)
                        effect.effectTypes.Add(enumConverter.EffectTypeStringToEnum(effectType));
                    if (effect.TriggerLocation != null)
                        foreach (string location in effect.TriggerLocation)
                            effect.triggerLocation.Add(enumConverter.LocationStringToEnum(location));
                    effect.activationLocation = enumConverter.LocationStringToEnum(effect.ActivationLocation);
                    if (effect.TargetLocation != null)
                        foreach (string location in effect.TargetLocation)
                            effect.targetLocation.Add(enumConverter.LocationStringToEnum(location));
                    if (effect.EffectTargetOwner != null)
                        foreach (string owner in effect.EffectTargetOwner)
                            effect.effectTargetController.Add(enumConverter.OwnerStringToEnum(owner));
                    if (effect.TargetingType != null)
                        foreach (string targetType in effect.TargetingType)
                            effect.targetingTypes.Add(enumConverter.TargetingTypeStringToEnum(targetType));
                    if (effect.EffectTargetType != null)
                        foreach (string type in effect.EffectTargetType)
                            effect.effectTargetType.Add(enumConverter.TypeStringToEnum(type));
                    if (effect.EffectTargetPlayType != null)
                        foreach (string playType in effect.EffectTargetPlayType)
                            effect.effectTargetPlayType.Add(enumConverter.PlayTypeStringToEnum(playType));
                    if (effect.TriggerState != null)
                        foreach (string state in effect.TriggerState)
                            effect.triggerState.Add(enumConverter.StateStringToEnum(state));
                    if (effect.TriggerPhase != null)
                        foreach (string phase in effect.TriggerPhase)
                            effect.triggerPhase.Add(enumConverter.PhaseStringToEnum(phase));
                    if (effect.TriggerOwner != null)
                        foreach (string owner in effect.TriggerOwner)
                            effect.triggerCardOwner.Add(enumConverter.OwnerStringToEnum(owner));
                    if (effect.TriggerCardLocation != null)
                        foreach (string location in effect.TriggerCardLocation)
                            effect.triggerCardLocation.Add(enumConverter.LocationStringToEnum(location));
                }

                //sets enums
                cardCloneCardLogic.type = enumConverter.TypeStringToEnum(cardCloneCardLogic.cardType);
                cardCloneCardLogic.playTypes = new(enumConverter.PlayTypeStringToEnumList(cardCloneCardLogic.cardType));

                if (cardCloneCardLogic.cardType != "god")
                    returnList.Add(cardCloneCardLogic);
            }
        }
        return returnList;
    }
}

