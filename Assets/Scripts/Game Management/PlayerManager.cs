using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayerManager : MonoBehaviour
{
	public Game_Manager gm;
    public UXManager ux;
    public SaveManager SaveManager;
    public PlayerManager enemy;
    public AIManager AIManager;

    public string PlayerName, PlayerID, DeckPath, HeroDeckPath;

    public bool isAI, isLocal, wentFirst;

    public List<string> deckListID = new();
    public List<string> heroDeckListID = new();

    public List<Card> deckList = new();
    public List<Card> heroDeckList = new();

    public List<CardLogic> handLogicList = new();
    public List<CardLogic> fieldLogicList = new();
    public List<CardLogic> deckLogicList = new();
    public List<CardLogic> heroDeckLogicList = new();
    public List<CardLogic> graveLogicList = new();
    public List<CardLogic> playableLogicList = new();
    public List<CardLogic> canAttackLogicList = new();
    public List<CardLogic> canUseEffectLogicList = new();

    public List<int> canUseEffectNumber = new();
    public List<int> canUseEffectSubNumber = new();

    public GodLogic heroCardLogic;

    public Transform activationZone;

    public GameObject deck, grave, hero, shield, shieldPanel, activateShieldButton, ignoreShieldButton, raycastBlocker, graveTarget, deckTarget, heroEffectTarget, heroAttackTarget, deckSearchButton, graveSearchButton, hourglassIcon;

    public GameObject[] cardSlots, handSlots, effectActivationButtons, attackDeclarationButtons, atkIcons, hpIcons, effectTargets, attackTargets, armorIcons, stealthIcons, tauntIcons, provokeIcons, spotIcons, bombIcons, burnIcons, poisonIcons, blindIcons, stunIcons, sleepIcons, camouflageIcons, shieldIcons, statusIcons, attackProjectileIcons, effectProjectileIcons;

    public bool[] isEmptyCardSlot, isEmptyHandSlot;

    public TMP_Text deckCountText, graveCountText, heroHpText, heroAtkText, shieldText, costText;

    public int deckCount, graveCount, shieldCount, costCount;

    public int costPhaseGain = 1;
    public int handSize;

    public enum Status
    {
        Heal, Damage, Death, HpLoss, HpGain, AtkLoss, AtkGain, Undefined
    }

    public void SetStatus(int orderNum, Status status, int statusChangeAmount)
    {
        GameObject stat = statusIcons[orderNum];
        switch(status)
        {
            case Status.Death:
            stat.SetActive(true);
            stat.GetComponent<Image>().sprite = ux.deathSprite;
                break;
            case Status.Damage:
                stat.SetActive(true);
                stat.GetComponent<Image>().color = Color.clear;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                stat.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                stat.GetComponentInChildren<TMP_Text>().color = Color.red;
                break;
            case Status.Heal:
                stat.SetActive(true);
                stat.GetComponent<Image>().color = Color.clear;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                stat.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                stat.GetComponentInChildren<TMP_Text>().color = Color.green;
                break;
            case Status.HpLoss:
                stat.SetActive(true);
                stat.GetComponent<Image>().sprite = ux.hpSprite;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                stat.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                stat.GetComponentInChildren<TMP_Text>().color = Color.red;
                break;
            case Status.HpGain:
                stat.SetActive(true);
                stat.GetComponent<Image>().sprite = ux.hpSprite;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                stat.GetComponentInChildren<TMP_Text>().text = "+" + statusChangeAmount.ToString();
                stat.GetComponentInChildren<TMP_Text>().color = Color.green;
                break;
            case Status.AtkLoss:
                stat.SetActive(true);
                stat.GetComponent<Image>().sprite = ux.atkSprite;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                stat.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                stat.GetComponentInChildren<TMP_Text>().color = Color.red;
                break;
            case Status.AtkGain:
                stat.SetActive(true);
                stat.GetComponent<Image>().sprite = ux.atkSprite;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                stat.GetComponentInChildren<TMP_Text>().text = "+" + statusChangeAmount.ToString();
                stat.GetComponentInChildren<TMP_Text>().color = Color.green;
                break;
            default:
                break;
        }
        
    }
}
