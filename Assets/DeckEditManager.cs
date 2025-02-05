using System;
using System.Collections.Generic;
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
    public Transform deckTransform;
    public GameObject deckEditCardImagePrefab;

    public void InitializeDeckEdit()
    {
        if (deckCards.Length == 0)
        {
            for(int i = 0; i < deckTransform.childCount; i++)
                new List<DeckEditCardImage>().Add(deckTransform.GetChild(i).GetComponent<DeckEditCardImage>());
            deckCards = new List<DeckEditCardImage>().ToArray();
        }
    }
    public void SetDeck(GodLogic godLogic, CardLogic[] powers, Deck deck)
    {
        this.godLogic = godLogic;
        this.deck = deck;
        godImage.sprite = godLogic.visualsLogic.image;
        for (int i = 0; i < powers.Length; i++)
            powerImages[i].SetLogic(powers[i], this);
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

    public void AddCardToDeck(CardLogic cardLogic)
    {
        DeckEditCardImage deckCard;
        if (deck.DeckList.Contains(cardLogic.dataLogic.id))
           deckCard = Array.Find(deckCards, x => x.cardLogic == cardLogic);
        else
            deckCard = Array.Find(deckCards, x => x.cardLogic == null);
    }
}
