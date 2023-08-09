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

    public GameObject deck, grave, hero, shield, shieldPanel, activateShieldButton, ignoreShieldButton, raycastBlocker, graveTarget, deckTarget, heroEffectTarget, heroAttackTarget, deckSearchButton, graveSearchButton, hourglassIcon, heroHpStatus, heroAtkStatus, heroNumbers, heroStatus, heroStatus2;

    public GameObject[] cardSlots, handSlots, effectActivationButtons, attackDeclarationButtons, atkIcons, hpIcons, effectTargets, attackTargets, armorIcons, stealthIcons, tauntIcons, provokeIcons, spotIcons, bombIcons, burnIcons, poisonIcons, blindIcons, stunIcons, sleepIcons, camouflageIcons, shieldIcons, statusIcons, statusIcons2, damageNums, attackProjectileIcons, effectProjectileIcons;

    public bool[] isEmptyCardSlot, isEmptyHandSlot;

    public TMP_Text deckCountText, graveCountText, heroHpText, heroAtkText, shieldText, costText;

    public int deckCount, graveCount, shieldCount, costCount;

    public int costPhaseGain = 1;
    public int handSize;

    public void SetStatus(int orderNum, Status status, int statusChangeAmount)
    {
        GameObject stat = statusIcons[orderNum];
        if (stat.GetComponent<StatusIconMoveAndFadeAway>().inUse)
            stat = statusIcons2[orderNum];
        GameObject num = damageNums[orderNum];
        switch (status)
        {
            case Status.Death:
                stat.SetActive(true);
                stat.GetComponent<Image>().sprite = ux.deathSprite;
                break;
            case Status.Damage:
                num.SetActive(true);
                num.GetComponent<Image>().color = Color.clear;
                num.transform.GetChild(0).gameObject.SetActive(true);
                num.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                num.GetComponentInChildren<TMP_Text>().color = Color.red;
                break;
            case Status.Heal:
                num.SetActive(true);
                num.GetComponent<Image>().color = Color.clear;
                num.transform.GetChild(0).gameObject.SetActive(true);
                num.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                num.GetComponentInChildren<TMP_Text>().color = Color.green;
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
                Debug.Log("Failed to assert status type");
                break;
        }
    }

    public void SetHeroStatus(Status status, int statusChangeAmount)
    {
        GameObject stat = heroStatus;
        if (stat.GetComponent<StatusIconMoveAndFadeAway>().inUse)
            stat = heroStatus2;
        switch (status)
        {
            case Status.Damage:
                heroNumbers.SetActive(true);
                heroNumbers.GetComponent<Image>().color = Color.clear;
                heroNumbers.transform.GetChild(0).gameObject.SetActive(true);
                heroNumbers.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                heroNumbers.GetComponentInChildren<TMP_Text>().color = Color.red;
                break;
            case Status.Heal:
                heroNumbers.SetActive(true);
                heroNumbers.GetComponent<Image>().color = Color.clear;
                heroNumbers.transform.GetChild(0).gameObject.SetActive(true);
                heroNumbers.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                heroNumbers.GetComponentInChildren<TMP_Text>().color = Color.green;
                break;
            case Status.HpLoss:
                heroHpStatus.SetActive(true);
                heroHpStatus.GetComponent<Image>().sprite = ux.hpSprite;
                heroHpStatus.transform.GetChild(0).gameObject.SetActive(true);
                heroHpStatus.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                heroHpStatus.GetComponentInChildren<TMP_Text>().color = Color.red;
                break;
            case Status.HpGain:
                heroHpStatus.SetActive(true);
                heroHpStatus.GetComponent<Image>().sprite = ux.hpSprite;
                heroHpStatus.transform.GetChild(0).gameObject.SetActive(true);
                heroHpStatus.GetComponentInChildren<TMP_Text>().text = "+" + statusChangeAmount.ToString();
                heroHpStatus.GetComponentInChildren<TMP_Text>().color = Color.green;
                break;
            case Status.AtkLoss:
                heroAtkStatus.SetActive(true);
                heroAtkStatus.GetComponent<Image>().sprite = ux.atkSprite;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                heroAtkStatus.GetComponentInChildren<TMP_Text>().text = "-" + statusChangeAmount.ToString();
                heroAtkStatus.GetComponentInChildren<TMP_Text>().color = Color.red;
                break;
            case Status.AtkGain:
                heroAtkStatus.SetActive(true);
                heroAtkStatus.GetComponent<Image>().sprite = ux.atkSprite;
                heroAtkStatus.transform.GetChild(0).gameObject.SetActive(true);
                heroAtkStatus.GetComponentInChildren<TMP_Text>().text = "+" + statusChangeAmount.ToString();
                heroAtkStatus.GetComponentInChildren<TMP_Text>().color = Color.green;
                break;
            default:
                Debug.Log("Failed to assert status type");
                break;
        }
    }
}
