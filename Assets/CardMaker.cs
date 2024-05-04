using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CardMaker : MonoBehaviour
{
    private const string capsPattern = "(?<=[a-z])([A-Z])";

    public GameObject cardPrefab;
    public GameObject effectDropdownPrefab;
    public GameObject card;
    public GameObject cardSelectionScreen;

    public GameObject newEffectButton;
    public GameObject costSetter;
    public GameObject atkSetter;
    public GameObject hpSetter;
    public GameObject costUpButton;
    public GameObject costDownButton;
    public GameObject atkUpButton;
    public GameObject atkDownButton;
    public GameObject hpUpButton;
    public GameObject hpDownButton;

    public GameObject removeTraitButton;
    public GameObject removePlayTypeButton;
    public GameObject removeEffectButton;
    public GameObject removeSubEffectButton;
    
    public bool autoID = true;
    public bool autoFill = true;

    public Card newCard;
    public CardLogic newCardLogic;
    public PlayableLogic newCardPlayableLogic;
    public CombatantLogic newCardCombatantLogic;

    public TMP_InputField nameInputField;
    public TMP_InputField effectInputField;
    public TMP_InputField flavourInputField;

    public TMP_Text cost;
    public TMP_Text atk;
    public TMP_Text hp;

    public TMP_Dropdown traitSelector;
    public TMP_Dropdown raritySelector;
    public TMP_Dropdown currentEffectSelector;

    public void Awake()
    {
        RarityDropDownRefresh();
        TraitDropDownRefresh();
    }

    public void RarityDropDownRefresh()
    {
        raritySelector.ClearOptions();
        foreach (Rarity r in (IEnumerable<Rarity>)Enum.GetValues(typeof(Rarity)))
            if (r != Rarity.Undefined)
                raritySelector.options.Add(new TMP_Dropdown.OptionData(Regex.Replace(r.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
        raritySelector.RefreshShownValue();
    }
    public void TraitDropDownRefresh()
    {
        traitSelector.ClearOptions();
        foreach (Trait t in (IEnumerable<Trait>)Enum.GetValues(typeof(Trait)))
            if (t != Trait.Undefined)
                traitSelector.options.Add(new TMP_Dropdown.OptionData(Regex.Replace(t.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
        traitSelector.RefreshShownValue();
    }

    public void ShowCardSelectionScreen() => cardSelectionScreen.SetActive(true);
    public void DisableCardSelectionScreen()=>cardSelectionScreen.SetActive(false);
    public void NewCard(string typeString)
    {
        RarityDropDownRefresh();
        TraitDropDownRefresh();
        Type type = Enum.Parse<Type>(typeString, true);
        bool isPlayable = false;
        bool isCombatant = false;
        DisableCardSelectionScreen();
        if(card != null) 
            Destroy(card);
        switch(type)
        {
            case Type.Spell:
                isPlayable = true; break;
            case Type.Fighter:
                isPlayable = true;
                isCombatant = true;
                break;
            case Type.God:
                isCombatant= true; break;
            default:
                Debug.Log("Unimplemented Type assertion attempted");
                break;

        }
        card = Instantiate(cardPrefab);
        card.SetActive(false);
        newCardLogic = card.AddComponent<CardLogic>();
        costSetter.SetActive(isPlayable);
        atkSetter.SetActive(isCombatant);
        hpSetter.SetActive(isCombatant);
        if (isPlayable)
            newCardPlayableLogic = card.AddComponent<PlayableLogic>();
        if (isCombatant)
            newCardCombatantLogic = card.AddComponent<CombatantLogic>();
        cost.text = 1.ToString();
        costDownButton.SetActive(false);
        costUpButton.SetActive(true);
        atk.text = 0.ToString(); 
        hp.text = 1.ToString();
        
        atkDownButton.SetActive(false);
        hpDownButton.SetActive(false);
        
        atkUpButton.SetActive(true);
        hpUpButton.SetActive(true);
        nameInputField.text = null;
        effectInputField.text = null;
        flavourInputField.text = null;
    }

    public void CostUp()
    {
        if (newCardPlayableLogic.cost < 9)
        {
            newCardPlayableLogic.cost++;
            costDownButton.SetActive(true);
        }
        else
        {
            newCardPlayableLogic.cost = 10;
            costUpButton.SetActive(false);
        }

        cost.text = newCardPlayableLogic.cost.ToString();
    }

    public void CostDown() 
    {
        if (newCardPlayableLogic.cost > 2)
        {
            newCardPlayableLogic.cost--;
            costUpButton.SetActive(true);
        }
        else
        {
            newCardPlayableLogic.cost = 1;
            costDownButton.SetActive(false);
        }

        cost.text = newCardPlayableLogic.cost.ToString();
    }
    public void AtkUp() 
    {
        if (newCardCombatantLogic.atk < 9)
        {
            newCardCombatantLogic.atk++;
            atkDownButton.SetActive(true);
        }
        else
        {
            newCardCombatantLogic.atk = 10;
            atkUpButton.SetActive(false);
        }
        atk.text = newCardCombatantLogic.atk.ToString();
    }
    public void AtkDown() 
    {
        if (newCardCombatantLogic.atk > 1)
        {
            newCardCombatantLogic.atk--;
            atkUpButton.SetActive(true);
        }
        else
        {
            newCardCombatantLogic.atk = 0;
            atkDownButton.SetActive(false);
        }
        atk.text = newCardCombatantLogic.atk.ToString();
    }
    public void HpUp() 
    {
        if (newCardCombatantLogic.maxHp < 10)
        {
            newCardCombatantLogic.maxHp++;
            hpDownButton.SetActive(true);
        }
        else
        {
            newCardCombatantLogic.maxHp = 11;
            hpUpButton.SetActive(false);
        }
        hp.text = newCardCombatantLogic.maxHp.ToString();
    }
    public void HpDown() 
    {
        if (newCardCombatantLogic.maxHp > 2)
        {
            newCardCombatantLogic.maxHp--;
            hpUpButton.SetActive(true);
        }
        else
        {
            newCardCombatantLogic.maxHp = 1;
            hpDownButton.SetActive(false);
        }
        hp.text = newCardCombatantLogic.maxHp.ToString();
    }
    public void CardNameSet() => newCardLogic.cardName = nameInputField.text;
    public void CardTextSet() => newCardLogic.cardText = effectInputField.text;
    public void CardFlavourTextSet() => newCardLogic.flavorText = flavourInputField.text;
    public void SetRarity(Rarity rarity) => newCardLogic.rarity = rarity;
    public void SetType(Type type) => newCardLogic.type = type;
    public void ToggleAutoID()
    {
        if (autoID)
            autoID = false;
        else
            autoID = true;
    }

    public void ToggleAutoFill()
    {
        if (autoFill)
            autoFill = false;
        else
            autoFill = true;
    }

    public void AddTrait(Trait trait)
    {
        if (!newCardLogic.traits.Contains(trait))
            newCardLogic.traits.Add(trait);
        if (newCardLogic.traits.Count > 0)
            removeTraitButton.SetActive(true);
    }
    public void RemoveTrait(Trait trait)
    {
        if(newCardLogic.traits.Contains(trait)) 
            newCardLogic.traits.Remove(trait);
        if (newCardLogic.traits.Count == 0)
            removeTraitButton.SetActive(false);
    }
    public void AddPlayType(PlayType playType)
    {
        if (!newCardLogic.playTypes.Contains(playType))
            newCardLogic.playTypes.Add(playType);
        if (newCardLogic.playTypes.Count > 0)
            removePlayTypeButton.SetActive(true);
    }
    public void RemovePlayType(PlayType playType)
    {
        if (newCardLogic.playTypes.Contains(playType))
            newCardLogic.playTypes.Remove(playType);
        if (newCardLogic.playTypes.Count == 0)
            removePlayTypeButton.SetActive(false);
    }
}
