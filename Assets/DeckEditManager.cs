using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditManager : MonoBehaviour
{
    [Header("Hero Panel")]
    public Image godImage;
    public GodLogic godLogic;
    public DeckEditCardImage[] powerImages;
    [Header("Hero Stat Texts")]
    public TMP_Text startingHandText;
    public TMP_Text drawForTurnText;
    public TMP_Text startingBloodText;
    public TMP_Text bloodForTurnText;
    public TMP_Text HPText;
    public TMP_Text ATKText;
    [Header("Attunement Panel")]
    public TMP_Text iraText;
    public TMP_Text amynaText;
    public TMP_Text enkrateiaText;
    public TMP_Text kataraText;
    public TMP_Text eulogiaText;
    public TMP_Text aretaText;

    public Deck deck;
    public DeckEditCardImage currentFocusDeckEditCardImage;
    public DeckEditCardImage[] deckCards;
    public DeckEditCardImage[] databaseCards;
    public Transform deckTransform;
    public Transform databaseTransform;
    public GameObject deckEditCardImagePrefab;
    public DeckManager deckManager;

    public void InitializeDeckEdit()
    {
        if (deckCards.Length == 0)
        {
            List<DeckEditCardImage> m = new();
            for (int i = 0; i < deckTransform.childCount; i++)
                m.Add(deckTransform.GetChild(i).GetComponent<DeckEditCardImage>());
            deckCards = m.ToArray();
        }
        if(databaseCards.Length < deckManager.database.Count)
        {
            List<DeckEditCardImage> m = new();
            for (int i = 0; i < databaseTransform.childCount; i++)
                m.Add(databaseTransform.GetChild(i).GetComponent<DeckEditCardImage>());
            if(m.Count < deckManager.database.Count)
                for (int i = m.Count; i < deckManager.database.Count; i++)
                    m.Add(Instantiate(deckEditCardImagePrefab, databaseTransform).GetComponent<DeckEditCardImage>());
            databaseCards = m.ToArray();
        }
        foreach (DeckEditCardImage deckCard in deckCards)
            deckCard.ToggleAddRemoveButtons(false);
        foreach (DeckEditCardImage databaseCard in databaseCards)
            databaseCard.ToggleAddRemoveButtons(false);
    }
    public void SetDeck(GodLogic godLogic, CardLogic[] powers, Deck deck)
    {
        this.godLogic = godLogic;
        if (deck != this.deck)
            foreach (DeckEditCardImage deckCard in deckCards)
            {
                int count = deckCard.cardCount;
                for (int i = 0; i < count; i++)
                    RemoveCardFromDeckScroll(deckCard.cardLogic);
            }
        this.deck = deck;
        godImage.sprite = godLogic.visualsLogic.image;
        for (int i = 0; i < powers.Length; i++)
            powerImages[i].SetLogic(powers[i], this, false);
        startingHandText.text = "5";
        drawForTurnText.text = "1";
        startingBloodText.text = "0";
        bloodForTurnText.text = "1";
        HPText.text = godLogic.combatantLogic.hp.ToString();
        ATKText.text = godLogic.combatantLogic.atk.ToString();
        SetAttunementValues(godLogic);
    }

    private void SetAttunementValues(GodLogic godLogic)
    {
        DisableAllAttunement();
        foreach (Attunement attunement in godLogic.dataLogic.attunements)
        {
            switch (attunement)
            {
                case Attunement.Ira:
                    TextSetter(iraText, attunement);
                    break;
                case Attunement.Amyna:
                    TextSetter(amynaText, attunement);
                    break;
                case Attunement.Enkrateia:
                    TextSetter(enkrateiaText, attunement);
                    break;
                case Attunement.Katara:
                    TextSetter(kataraText, attunement);
                    break;
                case Attunement.Eulogia:
                    TextSetter(eulogiaText, attunement);
                    break;
                case Attunement.Areta:
                   TextSetter(aretaText, attunement);
                    break;
                default:
                    throw new MissingReferenceException($"Attunement {attunement} not found in Attunement Panel");
            }
        }
    }
    private void TextSetter(TMP_Text text, Attunement attunement)
    {
        text.transform.parent.gameObject.SetActive(true);
        int index = godLogic.dataLogic.attunements.FindIndex(x => x == attunement);
        text.text = godLogic.attunementRates[index].ToString();
    }
    private void DisableAllAttunement()
    {
        iraText.transform.parent.gameObject.SetActive(false);
        amynaText.transform.parent.gameObject.SetActive(false);
        enkrateiaText.transform.parent.gameObject.SetActive(false);
        kataraText.transform.parent.gameObject.SetActive(false);
        eulogiaText.transform.parent.gameObject.SetActive(false);
        aretaText.transform.parent.gameObject.SetActive(false);
    }
    public void SetDeckCards(List<CardLogic> cards)
    {
        foreach(CardLogic cardLogic in cards)
            AddCardToDeckScroll(cardLogic);
    }
    public void AddCardToDeckScroll(CardLogic cardLogic)
    {
        DeckEditCardImage deckCard;
        CardOwnedID cardOwnedInDeck;
        try
        {
            deckCard = Array.Find(deckCards, x => x.cardLogic.dataLogic.id == cardLogic.dataLogic.id);
            cardOwnedInDeck = deck.DeckList.Find(x => x.ID == cardLogic.dataLogic.id);
        }
        catch
        {
            deckCard = null;
            cardOwnedInDeck = null;
        }
        CardOwnedID cardOwnedInDatabase = deckManager.unlockedCardIDs.Find(x => x.ID == cardLogic.dataLogic.id);
        if (cardOwnedInDatabase == null)
            return;
        if (deckCard == null)
        {
            deckCard = Array.Find(deckCards, x => !x.gameObject.activeSelf);
            if (deckCard == null)
                return;
            cardOwnedInDeck = new CardOwnedID();
            cardOwnedInDeck.SetID(cardLogic.dataLogic.id);
            deck.DeckList.Add(cardOwnedInDeck);
            deckCard.gameObject.name = cardLogic.dataLogic.cardName;
            deckCard.cardLogic = cardLogic;
            deckCard.amountOwned = cardOwnedInDatabase.Count;
            deckCard.cardCount = 1;
            deckCard.shouldShowAddButton = deckCard.amountOwned > 1;
            deckCard.shouldShowRemoveButton = true;
            deckCard.isDeckCard = true;
            deckCard.SetLogic(cardLogic, this, false);
        }
        else
        {
            deckCard.ToggleAddRemoveButtons(false);
            if (deckCard.amountOwned <= deckCard.cardCount)
                return;
            deckCard.cardCount++;
            deckCard.shouldShowAddButton = deckCard.amountOwned > deckCard.cardCount;

        }
        cardOwnedInDeck.SetCount(deckCard.cardCount);
        deckCard.gameObject.SetActive(true);
        deckCard.cardCountText.transform.parent.gameObject.SetActive(deckCard.cardCount > 1);
        deckCard.cardCountText.text = "x" + deckCard.cardCount.ToString();
        RemoveCardFromDatabaseScroll(cardLogic);
    }
    public void RemoveCardFromDeckScroll(CardLogic cardLogic)
    {
        DeckEditCardImage deckCard;
        CardOwnedID cardOwnedInDeck;
        try
        {
            deckCard = Array.Find(deckCards, x => x.cardLogic.dataLogic.id == cardLogic.dataLogic.id);
            cardOwnedInDeck = deck.DeckList.Find(x => x.ID == cardLogic.dataLogic.id);
        }
        catch
        {
            Debug.Log("Card not found in deck cards");
            return;
        }
        if (deckCard.cardCount <= 0)
            return;
        deckCard.cardCount--;
        cardOwnedInDeck.SetCount(deckCard.cardCount);
        if (cardOwnedInDeck.Count <= 0)
            deck.DeckList.Remove(cardOwnedInDeck);
        deckCard.gameObject.SetActive(deckCard.cardCount > 0);
        deckCard.shouldShowAddButton = deckCard.cardCount < deckCard.amountOwned;
        deckCard.cardCountText.transform.parent.gameObject.SetActive(deckCard.cardCount > 1);
        deckCard.cardCountText.text = "x" + deckCard.cardCount.ToString();
        AddCardToDatabaseScroll(cardLogic);
    }
    public void AddCardToDatabaseScroll(CardLogic cardLogic)
    {
        DeckEditCardImage databaseCard;
        try
        {
            databaseCard = Array.Find(databaseCards, x => x.cardLogic.dataLogic.id == cardLogic.dataLogic.id);
            databaseCard.gameObject.SetActive(true);
        }
        catch
        {
            databaseCard = null;
        }
        CardOwnedID cardOwnedInDatabase = deckManager.unlockedCardIDs.Find(x => x.ID == cardLogic.dataLogic.id);

        if (databaseCard == null)
        {
            databaseCard = Array.Find(databaseCards, x => !x.gameObject.activeSelf);
            if (databaseCard == null)
                databaseCard = Instantiate(deckEditCardImagePrefab, databaseTransform).GetComponent<DeckEditCardImage>();
            databaseCard.gameObject.name = cardLogic.dataLogic.cardName;
            databaseCard.cardLogic = cardLogic;

            databaseCard.amountOwned = cardOwnedInDatabase == null ? 0 : cardOwnedInDatabase.Count;
            databaseCard.cardCount = cardOwnedInDatabase == null ? 0 : databaseCard.amountOwned;
            databaseCard.shouldShowAddButton = databaseCard.cardCount > 1;
            databaseCard.shouldShowRemoveButton = databaseCard.cardCount < databaseCard.amountOwned;
            databaseCard.isDeckCard = false;
            databaseCard.SetLogic(cardLogic, this, cardOwnedInDatabase == null);
        }
        else
        {
            if (databaseCard.amountOwned <= databaseCard.cardCount)
                return;
            databaseCard.cardCount++;
            databaseCard.shouldShowRemoveButton = databaseCard.amountOwned > databaseCard.cardCount;
        }
        databaseCard.gameObject.SetActive(true);
        databaseCard.cardCountText.transform.parent.gameObject.SetActive(databaseCard.cardCount > 1);
        databaseCard.cardCountText.text = "x" + databaseCard.cardCount.ToString();
    }
    public void RemoveCardFromDatabaseScroll(CardLogic cardLogic)
    {
        DeckEditCardImage databaseCard = null;
        try
        {
            databaseCard = Array.Find(databaseCards, x => x.cardLogic.dataLogic.id == cardLogic.dataLogic.id);
        }
        catch
        {
            Debug.Log("Card not found in database cards");
            return;
        }
        if (databaseCard.cardCount <= 0)
            return;
        databaseCard.cardCount--;
        databaseCard.gameObject.SetActive(databaseCard.cardCount > 0);
        databaseCard.shouldShowRemoveButton = databaseCard.cardCount < databaseCard.amountOwned;
        databaseCard.cardCountText.transform.parent.gameObject.SetActive(databaseCard.cardCount > 1);
        databaseCard.cardCountText.text = "x" + databaseCard.cardCount.ToString();
    }
    public void SortDatabase()
    {
        Array.Sort(databaseCards, (a, b) =>
        {
            int countCompare = (a.amountOwned == 0).CompareTo(b.amountOwned == 0);
            if (countCompare != 0) return countCompare;
            return string.Compare(a.cardLogic.dataLogic.id, b.cardLogic.dataLogic.id, StringComparison.Ordinal);
        });
        for (int i = 0; i < databaseCards.Length; i++)
            databaseCards[i].transform.SetSiblingIndex(i);
    }
    public void UpdateDeck() => deckManager.UpdateDeck(deck);
}
