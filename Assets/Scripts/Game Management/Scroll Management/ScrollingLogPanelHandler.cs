using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class ScrollingLogPanelHandler : MonoBehaviour
{
    private const string capsPattern = "(?<=[a-z])([A-Z])";
    [SerializeField]
    private Game_Manager Game_Manager;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private List<GameLogHistoryEntry> gameLogHistoryEntries = new();

    [SerializeField]
    private GameObject logPrefabHolder;
    
    public GameObject targetScrollRayBlocker;
    
    [SerializeField]
    private ScrollingTargetsPanelHandler targetScroll;

    [SerializeField]
    private Sprite attackImage, arrowImage, effectImage, graveImage, handImage, heroDeckImage, deckImage, fieldImage, hpImage,atkImage;

    public bool isShowingTargets;
    public void ActivateTargetScroll(ScrollLogEntry scrollLogEntry)
    {
        EnableTargetScroll();
        targetScroll.ClearScrollCardsList();
        targetScroll.RemoveContentCards();
        targetScroll.AddCardListToScrollCards(scrollLogEntry.targets);
        targetScroll.AddContentCards();
    }

    public void EnableTargetScroll()
    {
        targetScroll.gameObject.SetActive(true);
        targetScrollRayBlocker.SetActive(true);
        isShowingTargets = true;
    }
    public void DisableTargetScroll()
    {
        targetScroll.gameObject.SetActive(false);
        targetScrollRayBlocker.SetActive(false);    
    }

    public void DeactivateTargetScroll()
    {
        DisableTargetScroll();
        isShowingTargets = false;
    }

    public void AddEntryToScrollEntries(GameLogHistoryEntry logHistoryEntry) => gameLogHistoryEntries.Add(logHistoryEntry);

    public void AddEntriesToScrollEntries(List<GameLogHistoryEntry> gameLogHistoryEntries) => 
        this.gameLogHistoryEntries.AddRange(gameLogHistoryEntries);

    public void ClearScrollEntries() => gameLogHistoryEntries.Clear();

    public void AddContentLogs()
    {
        foreach (GameLogHistoryEntry logHistoryEntry in gameLogHistoryEntries)
        {
            GameObject logEntry = Instantiate(logPrefabHolder, content);
            ScrollLogEntry scrollLogEntry = logEntry.GetComponent<ScrollLogEntry>();
            ScrollCardImage scrollCardImage = scrollLogEntry.cardHighlightImage.GetComponent<ScrollCardImage>();
            scrollLogEntry.historyEntry = logHistoryEntry;
            scrollLogEntry.Game_Manager = Game_Manager;
            scrollLogEntry.scrollingLogPanelHandler = this;
            scrollLogEntry.cardImage.sprite = logHistoryEntry.loggedCard.image;
            scrollLogEntry.TargetsButton.gameObject.SetActive(false);
            if (logHistoryEntry is AttackHistoryEntry attack)
            {
                scrollLogEntry.logTypeText.text = "Attack Declaration";
                scrollLogEntry.loggedText.text = $"{logHistoryEntry.loggedCard.cardName} Attacked {attack.attacker.cardName}";
                scrollLogEntry.SetTargetImage(true, attackImage, attack.attacker.image, LocationSprite(attack.attacker.currentLocation));
            }
            if (logHistoryEntry is CardStatusHistoryEntry status)
            {
                scrollLogEntry.logTypeText.text = "Status Applied";
                string s = status.cardStatus is Debuff ? " was inflicted " : " gained ";
                scrollLogEntry.loggedText.text = $"{logHistoryEntry.loggedCard.cardName}{s}{status.cardStatus}";
                scrollLogEntry.SetTargetImage(false, arrowImage, status.cardStatus.sprite, null);
            }
            if (logHistoryEntry is EffectLogHistoryEntry effect)
            {
                scrollLogEntry.logTypeText.text = "Effect Activation";
                string w;
                if (effect.effectTargets.Count == 1)
                {
                    scrollLogEntry.SetTargetImage(true, effectImage, effect.effectTargets[0].image,
                        LocationSprite(effect.effectTargets[0].currentLocation));
                    w = $"on {effect.effectTargets[0].cardName}";
                }
                else
                {
                    scrollLogEntry.TargetsButton.gameObject.SetActive(true);
                    w = effect.effectTargets.Count > 0 ? "on multiple targets" : "but there were no valid targets";
                    scrollLogEntry.TargetsButton.GetComponentInChildren<TMP_Text>().text = effect.effectTargets.Count.ToString();
                    targetScroll.TargetButton = scrollLogEntry.TargetsButton;
                    scrollLogEntry.targets.AddRange(effect.effectTargets);
                }
                string s = effect.loggedEffectUsed.ToString();
                string v = Regex.Replace(s, capsPattern, " $1", RegexOptions.Compiled).Trim();
                scrollLogEntry.loggedText.text = $"{logHistoryEntry.loggedCard.cardName} used effect {v} {w}";
            }
            if (logHistoryEntry is LocationHistoryEntry location)
            {
                scrollLogEntry.logTypeText.text = "Location Change";
                scrollLogEntry.loggedText.text = $"{logHistoryEntry.loggedCard.cardName}  was moved to {location.CurrentLocation}";
                scrollLogEntry.SetTargetImage(false, arrowImage, LocationSprite(location.CurrentLocation), null);
            }
            if (logHistoryEntry is StatChangeHistoryEntry stat)
            {
                scrollLogEntry.logTypeText.text = "Stat Modification";
                scrollLogEntry.SetTargetImage(false, arrowImage, StatusSprite(stat.StatusChange), null);
                scrollLogEntry.targetText.gameObject.SetActive(true);
                string s;
                if (stat.StatusChange == Status.HpLoss || stat.StatusChange == Status.AtkLoss || stat.StatusChange == Status.Damage)
                {
                    scrollLogEntry.targetText.text = $"-{stat.ChangeAmount}";
                    scrollLogEntry.targetText.color = Color.red;
                    s = " lost ";
                }
                else if (stat.StatusChange == Status.Heal)
                {
                    scrollLogEntry.targetText.text = $"+{stat.ChangeAmount}";
                    scrollLogEntry.targetText.color = Color.green;
                    s = " healed ";
                }
                else
                {
                    scrollLogEntry.targetText.text = $"+{stat.ChangeAmount}";
                    scrollLogEntry.targetText.color = Color.blue;
                    s = " gained ";
                }
                string z = scrollLogEntry.targetImage == hpImage ? " HP." : " ATK.";
                scrollLogEntry.loggedText.text = $"{logHistoryEntry.loggedCard.cardName}{s}{stat.ChangeAmount}{z}";
            }
            scrollCardImage.cardLogic = logHistoryEntry.loggedCard;
            scrollCardImage.Game_Manager = Game_Manager;
            scrollCardImage.highlightImage.color = Color.clear;
        }
    }
    public void RemoveContentLogs()
    {
        List<GameObject> allChildren = new();
        foreach (Transform child in content)
            allChildren.Add(child.gameObject);
        foreach (GameObject child in allChildren)
            Destroy(child);
        DisableTargetScroll();
    }

    public Sprite LocationSprite(Location location)
    {
        return location switch
        {
            Location.Deck => deckImage,
            Location.HeroDeck => heroDeckImage,
            Location.Hand => handImage,
            Location.Field => fieldImage,
            Location.Grave => graveImage,
            _ => null,
        };
    }

    public Sprite StatusSprite(Status status)
    {
        return status switch
        {
            Status.Heal => hpImage,
            Status.Damage => hpImage,
            Status.Death => hpImage,
            Status.HpLoss => hpImage,
            Status.HpGain => hpImage,
            Status.AtkLoss => atkImage,
            Status.AtkGain => atkImage,
            _ => null,
        };
    }    
}
