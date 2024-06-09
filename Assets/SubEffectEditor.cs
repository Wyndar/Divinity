using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class SubEffectEditor : MonoBehaviour
{
    private const string capsPattern = "(?<=[a-z])([A-Z])";

    [SerializeField] private EffectEditor EffectEditor;
    [SerializeField] private TMP_Dropdown effectTypeDropdown, effectUsedDropdown, controllerDropdown, targetingTypeDropdown;
    [SerializeField] private TMP_Text effectTypeText, effectUsedText, controllerText, targetingTypeText, targetTypesText,
        targetLocationsText;
    [SerializeField] private GameObject dropdownPrefab;

    [SerializeField] private Color warningColor;
    [SerializeField] private float warningTime;

    public Effect focusEffect;
    public SubEffect focusSubEffect;
    private TMP_Dropdown activeSelector;
    private GameObject removeButton;
    private TextScrollerHandler textScrollerHandler;
    private bool isAdd;
    public void SaveSubEffect()
    {
        if (effectTypeText.text == "" || effectUsedText.text == "" || controllerText.text == "")
            return;
    }
    public void LoadSubEffect()
    {
        effectTypeText.text = TextFormatter(focusSubEffect.effectType.ToString());
        effectUsedText.text = TextFormatter(focusSubEffect.effectUsed.ToString());
        controllerText.text = TextFormatter(focusSubEffect.effectTargetController.ToString());
        targetingTypeText.text = TextFormatter(focusSubEffect.targetingType.ToString());
        targetTypesText.text = TextFormatter(string.Join(" ,", focusSubEffect.effectTargetTypes.ToArray()));
        targetLocationsText.text = TextFormatter(string.Join(" ,", focusSubEffect.effectTargetTypes.ToArray()));
    }

    public void DisableScreen()
    {
        EffectEditor.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    public void EnableSelector(GameObject button)
    {
        if (activeSelector != null)
        {
            Warning(activeSelector.gameObject);
            return;
        }
        GameObject dropdownGO = Instantiate(dropdownPrefab, button.transform.parent, false);
        string parentName = dropdownGO.transform.parent.name;
        dropdownGO.name = parentName;
        dropdownGO.transform.parent = gameObject.transform;
        TMP_Dropdown dropdown = dropdownGO.GetComponent<TMP_Dropdown>();
        activeSelector = dropdown;
        isAdd = button.name.Contains("Add");
        dropdown.onValueChanged.AddListener(
                delegate { DropdownEditTrigger(); });
        if (parentName.Contains("Target Type"))
            DropDownRefresh(focusSubEffect.effectTargetTypes);
        else if (parentName.Contains("Target Location"))
            DropDownRefresh(focusSubEffect.targetLocations);
        else if (parentName.Contains("Effect Type"))
            DropDownRefresh<EffectTypes>(effectTypeDropdown, new((EffectTypes[])Enum.GetValues(typeof(EffectTypes))), null);
        else if (parentName.Contains("Effect Used"))
            DropDownRefresh<EffectsUsed>(effectUsedDropdown, new((EffectsUsed[])Enum.GetValues(typeof(EffectsUsed))), null);
        else if (parentName.Contains("Targeting Type"))
            DropDownRefresh<TargetingTypes>(targetingTypeDropdown, new((TargetingTypes[])Enum.GetValues(typeof(TargetingTypes))), null);
        else
            DropDownRefresh<Controller>(controllerDropdown, new((Controller[])Enum.GetValues(typeof(Controller))), null);
    }
    public void SetRemoveButton(GameObject gameObject) => removeButton = gameObject;
    public void SetScrollerHandler(TextScrollerHandler scrollerHandler) => textScrollerHandler = scrollerHandler;

    public void DisableSelector()
    {
        if (!activeSelector)
            return;
        activeSelector.onValueChanged.RemoveAllListeners();
        Destroy(activeSelector.gameObject);
        activeSelector = null;
    }

    //definitely should be a coroutine, change this later
    public void Warning(GameObject gameObject) => gameObject.GetComponent<Image>().CrossFadeColor(warningColor, warningTime, false, false);
    public void DropDownRefresh<T>(List<T> referenceList)
    {
        T[] referenceArray = (T[])Enum.GetValues(typeof(T));
        activeSelector.ClearOptions();
        activeSelector.options.Add(new(" "));
        List<T> list;
        if (isAdd)
            list = new(referenceArray);
        else
            list = new(referenceList);
        foreach (T l in list)
            if (l.ToString() != "Undefined" || l.ToString() != "Outside" || l.ToString() != "Limbo")
                if (!isAdd || !referenceList.Contains(l))
                    activeSelector.options.Add(new(TextFormatter(l.ToString())));
        activeSelector.RefreshShownValue();
    }
    public void DropDownRefresh<T>(TMP_Dropdown dropdown, List<T> referenceList, T[] referenceArray)
    {
        dropdown.ClearOptions();
        dropdown.options.Add(new(" "));
        List<T> list = referenceArray == null ? new(referenceList) : new(referenceArray);
        foreach (T l in list)
            if (l.ToString() != "Undefined"|| l.ToString() != "Outside" || l.ToString() != "Limbo")
                if (referenceArray == null || !referenceList.Contains(l))
                    dropdown.options.Add(new(TextFormatter(l.ToString())));
        dropdown.RefreshShownValue();
    }

    public void DropdownEditTrigger()
    {
        string parentName = activeSelector.transform.name;
        if (parentName.Contains("Target Type"))
            DropDownRefresh(focusSubEffect.effectTargetTypes);
        else if (parentName.Contains("Target Location"))
            DropDownRefresh(focusSubEffect.targetLocations);
        else if (parentName.Contains("Effect Type"))
            DropDownRefresh<EffectTypes>(effectTypeDropdown, new((EffectTypes[])Enum.GetValues(typeof(EffectTypes))), null);
        else if (parentName.Contains("Effect Used"))
            DropDownRefresh<EffectsUsed>(effectUsedDropdown, new((EffectsUsed[])Enum.GetValues(typeof(EffectsUsed))), null);
        else if (parentName.Contains("Targeting Type"))
            DropDownRefresh<TargetingTypes>(targetingTypeDropdown, new((TargetingTypes[])Enum.GetValues(typeof(TargetingTypes))), null);
        else
            DropDownRefresh<Controller>(controllerDropdown, new((Controller[])Enum.GetValues(typeof(Controller))), null);
    }

    public void SetInfoFromDropdown()
    {
        string infoString = activeSelector.options[activeSelector.value].text;
        if (infoString == "")
            return;
        if (activeSelector == effectTypeDropdown)
            focusSubEffect.effectType = Enum.Parse<EffectTypes>(infoString.Replace(" ", ""), true);
        else if(activeSelector == effectUsedDropdown)
            focusSubEffect.effectUsed = Enum.Parse<EffectsUsed>(infoString.Replace(" ", ""), true);
        else if(activeSelector == targetingTypeDropdown)
            focusSubEffect.targetingType = Enum.Parse<TargetingTypes>(infoString.Replace(" ", ""), true);
        else
            focusSubEffect.effectTargetController = Enum.Parse<Controller>(infoString.Replace(" ", ""), true);
    }

    public void EditInfoWithDropdown<T>(List<T> changeList)
    {
        string infoString = activeSelector.options[activeSelector.value].text;
        if (infoString == "")
            return;
        T info = (T)Enum.Parse(typeof(T), infoString.Replace(" ", ""), true);
        if (isAdd && !changeList.Contains(info))
        {
            changeList.Add(info);
            if (textScrollerHandler.scrollingTMP.text.Trim() == "")
                textScrollerHandler.scrollingTMP.text = infoString;
            else
                textScrollerHandler.scrollingTMP.text += $", {infoString}";
        }
        else if (!isAdd && changeList.Contains(info))
        {
            changeList.Remove(info);
            List<string> li = new(textScrollerHandler.scrollingTMP.text.Split(","));
            foreach (string s in li)
                if (infoString == s.Trim())
                {
                    li.Remove(s);
                    break;
                }
            textScrollerHandler.scrollingTMP.text = string.Join<string>(", ", li);
            textScrollerHandler.scrollingTMP.text.Replace("  ", " ");
        }
        removeButton.SetActive(changeList.Count > 0);
        DisableSelector();
    }
    public void EffectAmountUp()
    {
        if (focusSubEffect.effectTargetAmount < 10)
        {
            focusSubEffect.effectTargetAmount++;
            //hpDownButton.SetActive(true);
        }
        else
        {
            focusSubEffect.effectTargetAmount = 11;
            //hpUpButton.SetActive(false);
        }
        //hp.text = focusSubEffect.effectTargetAmount.ToString();
    }
    public void EffectAmountDown()
    {
        if (focusSubEffect.effectTargetAmount > 2)
        {
            focusSubEffect.effectTargetAmount--;
            //hpUpButton.SetActive(true);
        }
        else
        {
            focusSubEffect.effectTargetAmount = 1;
            //hpDownButton.SetActive(false);
        }
        //hp.text = focusSubEffect.effectTargetAmount.ToString();
    }
    private string TextFormatter(string s) => Regex.Replace(s, capsPattern, " $1", RegexOptions.Compiled).Trim();
}
