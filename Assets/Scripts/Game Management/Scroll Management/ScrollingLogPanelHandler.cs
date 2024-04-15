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
    
    [SerializeField]
    private ScrollingTargetsPanelHandler targetScroll;

    [SerializeField]
    private Sprite attackImage, arrowImage, effectImage, graveImage, handImage, heroDeckImage, deckImage, fieldImage, hpImage,atkImage;

    public bool isShowingTargets;
    public GameObject targetScrollRayBlocker;
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
            scrollLogEntry.cardImage.sprite = logHistoryEntry.loggedCard.isFaceDown ?
               logHistoryEntry.loggedCard.cardBack.GetComponent<SpriteRenderer>().sprite : logHistoryEntry.loggedCard.image;
            scrollLogEntry.TargetsButton.gameObject.SetActive(false);
            string cardName = logHistoryEntry.loggedCard.isFaceDown ? "Unknown Card" : logHistoryEntry.loggedCard.cardName;
            if (logHistoryEntry is AttackHistoryEntry attack)
            {
                scrollLogEntry.logTypeText.text = "Attack Declaration";
                scrollLogEntry.loggedText.text = $"{cardName} Attacked {attack.attacker.cardName}.";
                scrollLogEntry.SetTargetImage(true, attackImage, attack.attacker.image, LocationSprite(attack.attacker.currentLocation));
            }
            if (logHistoryEntry is CardStatusHistoryEntry status)
            {
                scrollLogEntry.logTypeText.text = "Status Applied";
                string s = status.cardStatus is Debuff ? "was inflicted" : "gained";
                scrollLogEntry.loggedText.text = $"{cardName} {s} {status.cardStatus}.";
                scrollLogEntry.SetTargetImage(false, arrowImage, status.cardStatus.sprite, null);
            }
            if (logHistoryEntry is EffectLogHistoryEntry effect)
            {
                scrollLogEntry.logTypeText.text = "Effect Activation";
                string w;
                string s = effect.loggedEffectUsed.ToString();
                if (effect.effectTargets.Count == 1)
                {
                    scrollLogEntry.SetTargetImage(true, effectImage, effect.effectTargets[0].image,
                        LocationSprite(effect.effectTargets[0].currentLocation));
                    string x = effect.effectTargets[0].isFaceDown ? "Unknown Card" : effect.effectTargets[0].cardName;
                    w = $"on {x}";
                }
                else
                {
                    switch (effect.loggedEffectUsed)
                    {
                        case EffectsUsed.Reinforce:
                        case EffectsUsed.Recruit:
                        case EffectsUsed.BloodRecovery:
                        case EffectsUsed.BloodBoost:
                            w = "";
                            scrollLogEntry.RemoveTargetImage();
                            break;
                        default:
                            scrollLogEntry.TargetsButton.gameObject.SetActive(true);
                            w = effect.effectTargets.Count > 0 ? "on multiple targets" : "but there were no valid targets";
                            scrollLogEntry.TargetsButton.GetComponentInChildren<TMP_Text>().text = effect.effectTargets.Count.ToString();
                            targetScroll.TargetButton = scrollLogEntry.TargetsButton;
                            scrollLogEntry.targets.AddRange(effect.effectTargets);
                            break;
                    }
                }
                string v = Regex.Replace(s, capsPattern, " $1", RegexOptions.Compiled).Trim();
                scrollLogEntry.loggedText.text = $"{cardName} used effect {v} {w}.";
            }
            if (logHistoryEntry is LocationHistoryEntry location)
            {
                scrollLogEntry.logTypeText.text = "Location Change";
                scrollLogEntry.loggedText.text = $"{cardName}  was moved to {location.CurrentLocation}.";
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
                    s = "lost";
                }
                else if (stat.StatusChange == Status.Heal)
                {
                    scrollLogEntry.targetText.text = $"+{stat.ChangeAmount}";
                    scrollLogEntry.targetText.color = Color.green;
                    s = "healed";
                }
                else
                {
                    scrollLogEntry.targetText.text = $"+{stat.ChangeAmount}";
                    scrollLogEntry.targetText.color = Color.blue;
                    s = "gained";
                }
                string z = scrollLogEntry.targetImage == hpImage ? "HP" : "ATK";
                scrollLogEntry.loggedText.text = $"{cardName} {s} {stat.ChangeAmount} {z}.";
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
