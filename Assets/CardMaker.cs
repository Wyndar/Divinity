//using UnityEngine;
//using TMPro;
//using System;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using UnityEngine.UI;

//public class CardMaker : MonoBehaviour
//{
//    [SerializeField] private EffectEditor EffectEditor;
//    [SerializeField] private CardSelector CardSelector;
//    [SerializeField] private TextScrollerHandler TextScrollerHandler;

//    [SerializeField] private GameObject newCardButton, costSetter, atkSetter, hpSetter, costUpButton, costDownButton, atkUpButton,
//        atkDownButton, hpUpButton, hpDownButton, traitAddButton, removeTraitButton;

//    [SerializeField] private bool autoID, autoFill = true;

//    [SerializeField] private TMP_InputField nameInputField, effectInputField, flavourInputField;

//    [SerializeField] private TMP_Text cost, atk, hp;

//    [SerializeField] private TMP_Dropdown traitRemoveSelector, traitAddSelector, effectSelector, raritySelector;

//    [SerializeField] private Color warningColor;
//    [SerializeField] private float warningTime;

//    private CardLogic newCardLogic;
//    private PlayableLogic newCardPlayableLogic;
//    private CombatantLogic newCardCombatantLogic;
//    private TMP_Dropdown activeSelector;

//    private const string capsPattern = "(?<=[a-z])([A-Z])";
//    private readonly List<Rarity> rarities = new((Rarity[])Enum.GetValues(typeof(Rarity)));
//    private readonly Trait[] traits = (Trait[])Enum.GetValues(typeof(Trait));

//    public void CardSelection()
//    {
//        CardSelector.gameObject.SetActive(true);
//        gameObject.SetActive(false);
//    }
//    public void EffectEditing()
//    {
//        EffectEditor.gameObject.SetActive(true);
//        gameObject.SetActive(false);
//    }

//    public void EnableEffectEditPanel()
//    {
//        if (newCardLogic.effects.Count > 0)
//        {
//            string effectNameString = effectSelector.options[effectSelector.value].text;
//            foreach (Effect effect in newCardLogic.effects)
//                if (effect.EffectName == effectNameString)
//                {
//                    EffectEditor.focusEffect = effect;
//                    break;
//                }
//        }
//        EffectEditing();
//    }

//    public void EnableSelector(TMP_Dropdown selector)
//    {
//        if (activeSelector != null)
//        {
//            Warning(activeSelector.gameObject);
//            return;
//        }
//        if (newCardLogic == null)
//        {
//            Warning(newCardButton);
//            return;
//        }
//        if (selector == traitRemoveSelector && newCardLogic.traits.Count == 0)
//        {
//            Warning(traitAddButton);
//            return;
//        }
//        if (selector == effectSelector && newCardLogic.effects.Count == 0)
//        {
//            EnableEffectEditPanel();
//            return;
//        }
//        DropDownRefresh(traitRemoveSelector, newCardLogic.traits, null);
//        DropDownRefresh(traitAddSelector, newCardLogic.traits, traits);
//        DropDownRefresh(raritySelector, rarities, null);
//        EffectSelectorRefresh();
//        selector.gameObject.SetActive(true);
//        activeSelector = selector;
//    }

//    public void DisableSelector()
//    {
//        if (!activeSelector)
//            return;
//        activeSelector.gameObject.SetActive(false);
//        activeSelector = null;
//    }

//    //definitely should be a coroutine, change this later
//    public void Warning(GameObject gameObject) => gameObject.GetComponent<Image>().CrossFadeColor(warningColor, warningTime, false, false);

//    public void EffectSelectorRefresh()
//    {
//        effectSelector.ClearOptions();
//        effectSelector.options.Add(new(" "));
//        if (newCardLogic.effects.Count > 0)
//            foreach (Effect effect in newCardLogic.effects)
//                effectSelector.options.Add(new(effect.EffectName.Trim()));
//        effectSelector.RefreshShownValue();
//    }
//    public void DropDownRefresh<T>(TMP_Dropdown dropdown, List<T> referenceList, T[] referenceArray)
//    {
//        dropdown.ClearOptions();
//        dropdown.options.Add(new(" "));
//        List<T> list = referenceArray == null ? new(referenceList) : new(referenceArray);
//        foreach (T l in list)
//            if (l.ToString() != "Undefined")
//                if (referenceArray == null || !referenceList.Contains(l))
//                    dropdown.options.Add(new(Regex.Replace(l.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
//        dropdown.RefreshShownValue();
//    }

//    public void NewCard(Type type, GameObject card)
//    {
//        TextScrollerHandler.scrollingTMP.text = "";
//        newCardLogic = card.AddComponent<CardLogic>();
//        newCardLogic.type = type;
//        bool isPlayable = false;
//        bool isCombatant = false;
//        switch (type)
//        {
//            case Type.Spell:
//                isPlayable = true; break;
//            case Type.Fighter:
//                isPlayable = true;
//                isCombatant = true;
//                break;
//            case Type.God:
//                isCombatant = true; break;
//            default:
//                throw new MissingReferenceException("Unimplemented Type assertion attempted");
//        }
//        costSetter.SetActive(isPlayable);
//        atkSetter.SetActive(isCombatant);
//        hpSetter.SetActive(isCombatant);
//        if (isPlayable)
//        {
//            newCardPlayableLogic = card.AddComponent<PlayableLogic>();
//            newCardLogic.playTypes.Add(PlayType.Playable);
//        }
//        if (isCombatant)
//        {
//            newCardCombatantLogic = card.AddComponent<CombatantLogic>();
//            newCardLogic.playTypes.Add(PlayType.Combatant);
//        }
//        cost.text = 1.ToString();
//        costDownButton.SetActive(false);
//        costUpButton.SetActive(true);
       
//        atk.text = 0.ToString(); 
//        hp.text = 1.ToString();
//        atkDownButton.SetActive(false);
//        hpDownButton.SetActive(false);
//        atkUpButton.SetActive(true);
//        hpUpButton.SetActive(true);

//        nameInputField.text = null;
//        effectInputField.text = null;
//        flavourInputField.text = null;
        
//        removeTraitButton.SetActive(false);
//        DropDownRefresh(raritySelector, rarities, null);
//        DisableSelector();
//    }

//    public void CostUp()
//    {
//        if (newCardPlayableLogic.cost < 9)
//        {
//            newCardPlayableLogic.cost++;
//            costDownButton.SetActive(true);
//        }
//        else
//        {
//            newCardPlayableLogic.cost = 10;
//            costUpButton.SetActive(false);
//        }

//        cost.text = newCardPlayableLogic.cost.ToString();
//    }

//    public void CostDown() 
//    {
//        if (newCardPlayableLogic.cost > 2)
//        {
//            newCardPlayableLogic.cost--;
//            costUpButton.SetActive(true);
//        }
//        else
//        {
//            newCardPlayableLogic.cost = 1;
//            costDownButton.SetActive(false);
//        }

//        cost.text = newCardPlayableLogic.cost.ToString();
//    }
//    public void AtkUp() 
//    {
//        if (newCardCombatantLogic.atk < 9)
//        {
//            newCardCombatantLogic.atk++;
//            atkDownButton.SetActive(true);
//        }
//        else
//        {
//            newCardCombatantLogic.atk = 10;
//            atkUpButton.SetActive(false);
//        }
//        atk.text = newCardCombatantLogic.atk.ToString();
//    }
//    public void AtkDown() 
//    {
//        if (newCardCombatantLogic.atk > 1)
//        {
//            newCardCombatantLogic.atk--;
//            atkUpButton.SetActive(true);
//        }
//        else
//        {
//            newCardCombatantLogic.atk = 0;
//            atkDownButton.SetActive(false);
//        }
//        atk.text = newCardCombatantLogic.atk.ToString();
//    }
//    public void HpUp() 
//    {
//        if (newCardCombatantLogic.maxHp < 10)
//        {
//            newCardCombatantLogic.maxHp++;
//            hpDownButton.SetActive(true);
//        }
//        else
//        {
//            newCardCombatantLogic.maxHp = 11;
//            hpUpButton.SetActive(false);
//        }
//        hp.text = newCardCombatantLogic.maxHp.ToString();
//    }
//    public void HpDown() 
//    {
//        if (newCardCombatantLogic.maxHp > 2)
//        {
//            newCardCombatantLogic.maxHp--;
//            hpUpButton.SetActive(true);
//        }
//        else
//        {
//            newCardCombatantLogic.maxHp = 1;
//            hpDownButton.SetActive(false);
//        }
//        hp.text = newCardCombatantLogic.maxHp.ToString();
//    }
//    public void CardNameSet() => newCardLogic.cardName = nameInputField.text;
//    public void CardTextSet() => newCardLogic.cardText = effectInputField.text;
//    public void CardFlavourTextSet() => newCardLogic.flavorText = flavourInputField.text;
//    public void SetRarity()
//    {
//        string rarityString = raritySelector.options[raritySelector.value].text;
//        if (rarityString == "")
//            return;
//        newCardLogic.rarity = Enum.Parse<Rarity>(rarityString, true);
//    }
//    public void ToggleAutoID()
//    {
//        if (autoID)
//            autoID = false;
//        else
//            autoID = true;
//    }

//    public void ToggleAutoFill()
//    {
//        if (autoFill)
//            autoFill = false;
//        else
//            autoFill = true;
//    }

//    //taking extra unused param to bypass the delegate issues
//    public void DropdownChange(TMP_Dropdown dropdown) => DropdownEditTrigger(dropdown, rarities);

//    private void DropdownEditTrigger<T>(TMP_Dropdown dropdown, List<T> changeList)
//    {
//        if (dropdown == traitAddSelector || dropdown == traitRemoveSelector)
//            EditInfoWithDropdown(dropdown, newCardLogic.traits, removeTraitButton,
//                TextScrollerHandler, dropdown == traitAddSelector);
//    }    

//    public void EditInfoWithDropdown<T>(TMP_Dropdown dropdown, List<T> changeList, GameObject removeButton, TextScrollerHandler textScroller, bool add)
//    {
//        string infoString = dropdown.options[dropdown.value].text;
//        if (infoString == "")
//            return;
//        T info = (T)Enum.Parse(typeof(T), infoString.Replace(" ", ""), true);
//        if (add && !changeList.Contains(info))
//        {
//            changeList.Add(info);
//            if (textScroller.scrollingTMP.text.Trim() == "")
//                textScroller.scrollingTMP.text = infoString;
//            else
//                textScroller.scrollingTMP.text += $", {infoString}";
//        }
//        else if (!add && changeList.Contains(info))
//        {
//            changeList.Remove(info);
//            List<string> li = new(TextScrollerHandler.scrollingTMP.text.Split(","));
//            foreach (string s in li)
//                if (infoString == s.Trim())
//                {
//                    li.Remove(s);
//                    break;
//                }
//            TextScrollerHandler.scrollingTMP.text = string.Join<string>(", ", li);
//            TextScrollerHandler.scrollingTMP.text.Replace("  ", " ");
//        }
//        removeButton.SetActive(changeList.Count > 0);
//        DisableSelector();
//    }
//}
