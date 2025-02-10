using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrollingLogPanelHandler : GenericScrollingPanelHandler<GameLogHistoryEntry>
{
    [SerializeField] private GameBattleManager Game_Manager;
    [SerializeField] private GameObject logPrefabHolder;
    [SerializeField] private ScrollingTargetsPanelHandler targetScroll;
    [SerializeField] private GameObject targetScrollRayBlocker;

    public bool isShowingTargets;

    protected override void SetupImage(GameObject imageObject, GameLogHistoryEntry logHistoryEntry)
    {
        ScrollLogEntry scrollLogEntry = imageObject.GetComponent<ScrollLogEntry>();
        scrollLogEntry.historyEntry = logHistoryEntry;
        scrollLogEntry.Game_Manager = Game_Manager;
        scrollLogEntry.scrollingLogPanelHandler = this;
        scrollLogEntry.cardImage.sprite = logHistoryEntry.loggedCard.visualsLogic.isFaceDown
            ? logHistoryEntry.loggedCard.visualsLogic.cardBack.GetComponent<SpriteRenderer>().sprite
            : logHistoryEntry.loggedCard.visualsLogic.image;
    }

    public void ActivateTargetScroll(ScrollLogEntry scrollLogEntry)
    {
        EnableTargetScroll();
        targetScroll.ClearItems();
        targetScroll.RemoveContent();
        targetScroll.AddItemList(scrollLogEntry.targets);
        targetScroll.AddContent();
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
        isShowingTargets = false;
    }
}
