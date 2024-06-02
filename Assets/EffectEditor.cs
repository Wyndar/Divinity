using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;
using System;
using System.Collections;

public class EffectEditor : MonoBehaviour
{
    private const string capsPattern = "(?<=[a-z])([A-Z])";

    [SerializeField] private CardMaker CardMaker;
    [SerializeField] private TMP_InputField effectNameInputField;
    [SerializeField] private TMP_Dropdown controllerDropdown;
    [SerializeField] private GameObject dropdownPrefab;

    [SerializeField] private Color warningColor;
    [SerializeField] private float warningTime;

    public Effect focusEffect;
    public SubEffect focusSubEffect;
    private TMP_Dropdown activeSelector;
    private GameObject removeButton;
    private TextScrollerHandler textScrollerHandler;
    private bool isAdd;

    public void DisableScreen()
    {
        CardMaker.gameObject.SetActive(true);
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
        TMP_Dropdown dropdown = dropdownGO.GetComponent<TMP_Dropdown>();
        activeSelector = dropdown;
        isAdd = button.name.Contains("Add");
        dropdown.onValueChanged.AddListener(
                delegate { DropdownEditTrigger(); });
        if (parentName.Contains("Activation Location"))
            DropDownRefresh(focusEffect.activationLocations);
        else if (parentName.Contains("Trigger Location"))
            DropDownRefresh(focusEffect.TriggerLocations);
        else if (parentName.Contains("Trigger Card Location"))
            DropDownRefresh(focusEffect.TriggerCardLocations);
        else if (parentName.Contains("Type"))
            DropDownRefresh(focusEffect.triggerCardTypes);
        else if (parentName.Contains("State"))
            DropDownRefresh(focusEffect.triggerStates);
        else if (parentName.Contains("Phase"))
            DropDownRefresh(focusEffect.triggerPhases);
        else if (parentName.Contains("Trigger Effect"))
            DropDownRefresh(focusEffect.triggerEffects);
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
            if (l.ToString() != "Undefined"|| l.ToString() != "Outside"|| l.ToString() != "Limbo")
                if (!isAdd || !referenceList.Contains(l))
                    activeSelector.options.Add(new(Regex.Replace(l.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
        activeSelector.RefreshShownValue();
    }
    public void DropDownRefresh<T>(TMP_Dropdown dropdown, List<T> referenceList, T[] referenceArray)
    {
        dropdown.ClearOptions();
        dropdown.options.Add(new(" "));
        List<T> list = referenceArray == null ? new(referenceList) : new(referenceArray);
        foreach (T l in list)
            if (l.ToString() != "Undefined")
                if (referenceArray == null || !referenceList.Contains(l))
                    dropdown.options.Add(new(Regex.Replace(l.ToString(), capsPattern, " $1", RegexOptions.Compiled).Trim()));
        dropdown.RefreshShownValue();
    }

    public void DropdownEditTrigger()
    {
        string parentName = activeSelector.transform.parent.name;
        if (parentName.Contains("Activation Location"))
            EditInfoWithDropdown(focusEffect.activationLocations);
        if (parentName.Contains("Trigger Location"))
            EditInfoWithDropdown(focusEffect.TriggerLocations);
        if (parentName.Contains("Trigger Card Location"))
            EditInfoWithDropdown(focusEffect.TriggerCardLocations);
        else if (parentName.Contains("Type"))
            EditInfoWithDropdown(focusEffect.triggerCardTypes);
        else if (parentName.Contains("State"))
            EditInfoWithDropdown(focusEffect.triggerStates);
        else if (parentName.Contains("Phase"))
            EditInfoWithDropdown(focusEffect.triggerPhases);
        else if (parentName.Contains("Trigger Effect"))
            EditInfoWithDropdown(focusEffect.triggerEffects);
    }

    public void SetController()
    {
        string controllerString = controllerDropdown.options[controllerDropdown.value].text;
        if (controllerString == "")
            return;
        focusEffect.triggerCardOwner = Enum.Parse<Controller>(controllerString, true);
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
    public void SubEffectsEditing()
    {
        // focusSubEffect = focusEffect.SubEffects ??= new List<SubEffect> { new() };
    }

    public void EffectNameSet() => focusEffect.EffectName = effectNameInputField.text;
}
