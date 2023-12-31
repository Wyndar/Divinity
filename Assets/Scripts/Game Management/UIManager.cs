using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public Game_Manager gm;
    public Sprite targetSprite, provokeSprite, deathSprite, tauntSprite, buffSprite, hpSprite, atkSprite, bombSprite, stunSprite, armorSprite;
    public GameObject fieldIconPrefab;
    public void UIUpdate(PlayerManager playerManager)
    {
        playerManager.heroCardLogic.OnFieldAtkRefresh();
        playerManager.heroCardLogic.OnFieldHpRefresh();
        playerManager.deckCount = playerManager.deckLogicList.Count;
        playerManager.graveCount = playerManager.graveLogicList.Count;
        playerManager.shieldCount = playerManager.heroDeckLogicList.Count;

        playerManager.deckCountText.text = playerManager.deckCount.ToString();
        playerManager.graveCountText.text = playerManager.graveCount.ToString();
        playerManager.shieldText.text = playerManager.shieldCount.ToString();
        playerManager.costText.text = playerManager.costCount.ToString();
    }

    public void StatUpdate(Status status, int statusChangeAmount, GameObject stat, GameObject num)
    {
        Color numColour;
        string signText;

        switch (status)
        {
            case Status.Damage:
            case Status.AtkLoss:
            case Status.HpLoss:
                numColour = Color.red;
                signText = "-";
                break;
            case Status.Heal:
            case Status.HpGain:
            case Status.AtkGain:
                numColour = Color.green;
                signText = "+";
                break;
            default:
                numColour = Color.clear;
                signText = " ";
                break;
        }

        switch (status)
        {
            case Status.Death:
                stat.SetActive(true);
                stat.GetComponent<Image>().sprite = deathSprite;
                break;
            case Status.Damage:
            case Status.Heal:
                num.SetActive(true);
                num.GetComponent<Image>().color = Color.clear;
                num.transform.GetChild(0).gameObject.SetActive(true);
                num.GetComponentInChildren<TMP_Text>().text = signText + statusChangeAmount.ToString();
                num.GetComponentInChildren<TMP_Text>().color = numColour;
                break;
            case Status.HpLoss:
            case Status.HpGain:
                stat.GetComponent<Image>().sprite = hpSprite;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                stat.GetComponentInChildren<TMP_Text>().text = signText + statusChangeAmount.ToString();
                stat.GetComponentInChildren<TMP_Text>().color = numColour;
                break;
            case Status.AtkLoss:
            case Status.AtkGain:
                stat.SetActive(true);
                stat.GetComponent<Image>().sprite = atkSprite;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                stat.GetComponentInChildren<TMP_Text>().text = signText + statusChangeAmount.ToString();
                stat.GetComponentInChildren<TMP_Text>().color = numColour;
                break;
            default:
                Debug.Log("Failed to assert status type");
                break;
        }
    }

    public void HeroStatUpdate(Status status, int statusChangeAmount, GameObject stat, PlayerManager player)
    {
        GameObject heroNumbers = player.heroNumbers;
        GameObject heroHpStatus = player.heroHpStatus;
        GameObject heroAtkStatus = player.heroAtkStatus;
        Color numColour;
        string signText;

        switch (status)
        {
            case Status.Damage:
            case Status.AtkLoss:
            case Status.HpLoss:
                numColour = Color.red;
                signText = "-";
                break;
            case Status.Heal:
            case Status.HpGain:
            case Status.AtkGain:
                numColour = Color.green;
                signText = "+";
                break;
            default:
                numColour = Color.clear;
                signText = " ";
                break;
        }
        switch (status)
        {
            case Status.Damage:
            case Status.Heal:
                heroNumbers.SetActive(true);
                heroNumbers.GetComponent<Image>().color = Color.clear;
                heroNumbers.transform.GetChild(0).gameObject.SetActive(true);
                heroNumbers.GetComponentInChildren<TMP_Text>().text = signText + statusChangeAmount.ToString();
                heroNumbers.GetComponentInChildren<TMP_Text>().color = numColour;
                break;
            case Status.HpLoss:
            case Status.HpGain:
                heroHpStatus.SetActive(true);
                heroHpStatus.GetComponent<Image>().sprite = hpSprite;
                stat.transform.GetChild(0).gameObject.SetActive(true);
                heroHpStatus.GetComponentInChildren<TMP_Text>().text = signText + statusChangeAmount.ToString();
                heroHpStatus.GetComponentInChildren<TMP_Text>().color = numColour;
                break;
            case Status.AtkGain:
            case Status.AtkLoss:
                heroAtkStatus.SetActive(true);
                heroAtkStatus.GetComponent<Image>().sprite = atkSprite;
                heroAtkStatus.transform.GetChild(0).gameObject.SetActive(true);
                heroAtkStatus.GetComponentInChildren<TMP_Text>().text = signText + statusChangeAmount.ToString();
                heroAtkStatus.GetComponentInChildren<TMP_Text>().color = numColour;
                break;
            default:
                Debug.Log("Failed to assert status type");
                break;
        }
    }

    public void StatIconUpdate(GameObject icon, CardStatus status)
    {
        if (icon.transform.childCount > 3)
            return;
        GameObject prefabGO = Instantiate(fieldIconPrefab, icon.transform, false);
        prefabGO.GetComponent<FieldIconHolder>().cardStatus = status;
        status.fieldIconHolder = prefabGO.GetComponent<FieldIconHolder>();
        prefabGO.GetComponentInChildren<TMP_Text>().text = status.shouldCountdown?status.Timer.ToString():status.Amount>0?status.Amount.ToString():" ";
        prefabGO.GetComponent<Image>().color = status is Buff ? Color.blue : Color.red;

        if (status is Buff b)
            switch (b.buff)
            {
                case Buffs.Armor:
                    prefabGO.GetComponent<Image>().sprite = armorSprite;
                    break;
                default:
                    Debug.Log("Failed to find sprite");
                    break;
            }
        else if(status is Debuff d)
            switch(d.debuff) 
            {
                case Debuffs.Bombed:
                    prefabGO.GetComponent<Image>().sprite = bombSprite;
                    break;
                case Debuffs.Stunned:
                    prefabGO.GetComponent<Image>().sprite = stunSprite;
                    break;
                default:
                    Debug.Log("Failed to find sprite");
                    break;
            }
    }
}

