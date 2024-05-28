using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Linq;

public class CardMaker : MonoBehaviour
{
    private const string capsPattern = "(?<=[a-z])([A-Z])";

    public TextScrollerHandler TextScrollerHandler;

    public GameObject cardPrefab;
    public GameObject effectDropdownPrefab;
    public GameObject card;
    public GameObject cardSelectionScreen;
    public GameObject effectEditingScreen;

    public GameObject addNewEffectButton;
    public GameObject removeNewEffectButton;
    public GameObject effectEditButton;
    public GameObject newCardButton;
    public GameObject costSetter;
    public GameObject atkSetter;
    public GameObject hpSetter;
    public GameObject costUpButton;
    public GameObject costDownButton;
    public GameObject atkUpButton;
    public GameObject atkDownButton;
    public GameObject hpUpButton;
    public GameObject hpDownButton;

    public GameObject traitAddButton;
    public GameObject removeTraitButton;
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

    public TMP_Dropdown traitRemoveSelector;
    public TMP_Dropdown traitAddSelector;
    public TMP_Dropdown effectRemoveSelector;
    public TMP_Dropdown effectAddSelector;
    public TMP_Dropdown raritySelector;

    public GameObject activeSelector;
    public Color warningColor;
    public float warningTime;

    private Effect focusEffect;
    private SubEffect focusSubEffect;
    public void Awake()
    {
        RarityDropDownRefresh();
        removeTraitButton.SetActive(false);
        EnableSelector(cardSelectionScreen);
    }
    public void EffectSelectorEnable()
    {
        focusEffect ??= new Effect();

        //EffectTypeDropDownRefresh(effectRemoveSelector, newCardLogic.);
        EffectTypeDropDownRefresh(effectAddSelector, (IEnumerable<EffectTypes>)Enum.GetValues(typeof(EffectTypes)));
    }

    public void SubEffectsEditing()
    {
       // focusSubEffect = focusEffect.SubEffects ??= new List<SubEffect> { new() };
    }

    public void EnableSelector(GameObject gameObject)
    {
        if (activeSelector != null)
        {
            Warning(activeSelector);
            return;
        }
        if (gameObject == cardSelectionScreen || gameObject == effectEditingScreen)
        {
            gameObject.SetActive(true);
            activeSelector = gameObject;
            return;
        }
        if (newCardLogic == null)
        {
            Warning(newCardButton);
            return;
        }
        if (gameObject == traitRemoveSelector && newCardLogic.traits.Count == 0)
        {
            Warning(traitAddButton);
            return;
        }
        TraitDropDownRefresh(traitRemoveSelector, newCardLogic.traits);
        TraitDropDownRefresh(traitAddSelector, (IEnumerable<Trait>)Enum.GetValues(typeof(Trait)));
        gameObject.SetActive(true);
        activeSelector = gameObject;
    }

    public void DisableSelector()
    {
        activeSelector.SetActive(false);
        activeSelector = null;
    }    

    //definitely should be a coroutine, change this later
    public void Warning(GameObject gameObject) => gameObject.GetComponent<Image>().CrossFadeColor(warningColor, warningTime, false, false);

    public void RarityDropDownRefresh()
    {
        raritySelector.ClearOptions();
        raritySelector.options.Add(new(" "));
        foreach (Rarity r in (IEnumerable<Rarity>)Enum.GetValues(typeof(Rarity)))
            if (r != Rarity.Undefined)
                raritySelector.options.Add(new(Regex.Replace(r.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
        raritySelector.RefreshShownValue();
    }
    public void TraitDropDownRefresh(TMP_Dropdown dropdown, IEnumerable<Trait> traits)
    {
        dropdown.ClearOptions();
        dropdown.options.Add(new(" "));
        foreach (Trait t in traits)
            if (t != Trait.Undefined)
                if (dropdown != traitAddSelector || !newCardLogic.traits.Contains(t))
                    dropdown.options.Add(new(Regex.Replace(t.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
        dropdown.RefreshShownValue();
    }

    public void EffectTypeDropDownRefresh(TMP_Dropdown dropdown, IEnumerable<EffectTypes> traits)
    {
        dropdown.ClearOptions();
        dropdown.options.Add(new(" "));
        foreach (EffectTypes t in traits)
            if (t != EffectTypes.Undefined)
                dropdown.options.Add(new(Regex.Replace(t.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
        dropdown.RefreshShownValue();
    }

    public void EffectUsedDropDownRefresh(TMP_Dropdown dropdown, IEnumerable<EffectsUsed> traits)
    {
        dropdown.ClearOptions();
        dropdown.options.Add(new(" "));
        foreach (EffectsUsed t in traits)
            if (t != EffectsUsed.Undefined)
                    dropdown.options.Add(new(Regex.Replace(t.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
        dropdown.RefreshShownValue();
    }

    public void NewCard(string typeString)
    {
        RarityDropDownRefresh();
        Type type = Enum.Parse<Type>(typeString, true);
        bool isPlayable = false;
        bool isCombatant = false;
        DisableSelector();
        TextScrollerHandler.scrollingTMP.text = "";
        if (card != null) 
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
                throw new MissingReferenceException("Unimplemented Type assertion attempted");
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

    public void AddTrait()
    {
        string traitString = traitAddSelector.options[traitAddSelector.value].text;
        if (traitString == "")
            return;
        Trait trait = Enum.Parse<Trait>(traitString.Replace(" ", ""), true);
        newCardLogic.traits ??= new();
        if(!newCardLogic.traits.Contains(trait))
        {
            newCardLogic.traits.Add(trait);
            if (TextScrollerHandler.scrollingTMP.text.Trim() == "")
                TextScrollerHandler.scrollingTMP.text = traitString;
            else
                TextScrollerHandler.scrollingTMP.text += $", {traitString}";
            DisableSelector();
        }
        if (newCardLogic.traits.Count > 0)
            removeTraitButton.SetActive(true);
    }
    public void RemoveTrait()
    {
        string traitString = traitRemoveSelector.options[traitRemoveSelector.value].text;
        Trait trait = Enum.Parse<Trait>(traitString.Replace(" ", ""), true);
        if (newCardLogic.traits.Contains(trait))
        {
            newCardLogic.traits.Remove(trait);
            List<string>li = new(TextScrollerHandler.scrollingTMP.text.Split(","));
            foreach (string s in li)
                if (traitString == s.Trim())
                {
                    li.Remove(s);
                    break;
                }
            TextScrollerHandler.scrollingTMP.text = string.Join<string>(", ", li);
            TextScrollerHandler.scrollingTMP.text.Replace("  ", " ");
            DisableSelector();
        }
        if (newCardLogic.traits.Count == 0)
            removeTraitButton.SetActive(false);
    }
}
