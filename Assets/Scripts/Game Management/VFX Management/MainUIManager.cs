﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] private Game_Manager gm;

    [SerializeField] private InputManager InputManager;

    [SerializeField] private AudioManager audioManager;

    [SerializeField] private ToolTipManager toolTipManager;

    [SerializeField] private ScrollingCardPanelHandler scrollingCardPanelHandler;

    [SerializeField] private ScrollingLogPanelHandler scrollingLogPanelHandler;

    [SerializeField] private ScrollingStatusPanelHandler infoScrollingStatusPanelHandler;

    [SerializeField] private GameObject trail, effectPanel, infoPanelStatusBar, infoPanel, infoPanelStats, infoPanelEnergy,
        rayBlocker, cardScrollScreen, gameLogScrollScreen, effectActivationPanel, cardScrollScreenButton, gameLogScreenRayBlocker,
        cardScrollRayBlocker, statScrollRayBlocker, gameOverPanel, gameLogButton;

    [SerializeField] private TMP_Text infoPanelNameText, infoPanelAtkText, infoPanelHpText, infoPanelCostText,
        infoPanelEffectText, infoPanelFlavourText, effectPanelNameText, effectActivationPanelText, gameOverWinnerText;

    [SerializeField] private Image infoPanelImage;

    [SerializeField] private TMP_Text[] effectPanelTexts;

    [SerializeField] private GameObject[] effects, switchButtons, activateButtons;

    [SerializeField] private string effectActivationText;

    public Vector2 touchStartPosition, touchEndPosition;
    private float touchStartTime, touchEndTime;
    private Coroutine trailCoroutine;
    private List<string> effectText;
    private bool hasRaycast;
    private bool allowCardLogicSwap = true;
    private bool isUsingshield;

    private void OnEnable()
    {
        InputManager.OnStartTouch += TouchStart;
        InputManager.OnEndTouch += TouchEnd;
    }
    private void OnDisable()
    {
        InputManager.OnEndTouch -= TouchStart;
        InputManager.OnStartTouch -= TouchEnd;
    }

    private void TouchStart(Vector2 screenPosition, float time)
    {
        audioManager.NewAudioPrefab(audioManager.click);
        if (gm.currentFocusCardLogic != null)
        {
            if (gm.currentFocusCardLogic.TryGetComponent(out PlayableLogic playableLogic))
                playableLogic.DisableHover();
            gm.currentFocusCardLogic.DisableCardOutline();
        }
        touchStartPosition = ScreenToWorld(screenPosition);
        touchStartTime = time;
        trail.SetActive(true);
        trail.transform.position = ScreenToWorld(screenPosition);
        trailCoroutine = StartCoroutine(Trail(screenPosition));
    }

    private IEnumerator Trail(Vector2 screenPosition)
    {
        if (hasRaycast == false)
        {
            hasRaycast = true;
            RaycastTargeting(screenPosition);
        }
        yield return new WaitForSeconds(0.2f);
        while (rayBlocker.activeInHierarchy == false)
        {
            trail.transform.position = ScreenToWorld(InputManager.CurrentFingerPosition);
            if (gm.currentFocusCardLogic != null)
                if (gm.currentFocusCardLogic.currentLocation == Location.Hand && gm.currentPhase == Phase.MainPhase && gm.turnPlayer == gm.currentFocusCardLogic.cardController)
                    gm.currentFocusCardLogic.transform.position = ScreenToWorld(InputManager.CurrentFingerPosition);
            yield return null;
        }
    }

    private void TouchEnd(Vector2 screenPosition, float time)
    {
        trail.SetActive(false);
        StopCoroutine(trailCoroutine);
        touchEndPosition = ScreenToWorld(screenPosition);
        touchEndTime = time;
        if (gm.gameState != GameState.Open)
        {
            hasRaycast = false;
            return;
        }
        if (gm.currentFocusCardLogic != null)
        {
            gm.currentFocusCardLogic.TryGetComponent(out PlayableLogic playableLogic);
            //swipe check to play
            if (touchEndTime - touchStartTime > 0.1 && touchEndTime - touchStartTime < 1 && Vector2.Distance(touchEndPosition, touchStartPosition) >= 3f)
            {
                float playDist = Mathf.Abs(touchEndPosition.x - touchStartPosition.x);
                if (playDist > 2f && gm.currentPhase == Phase.MainPhase && playableLogic != null)
                {
                    playableLogic.DisableHover();
                    playableLogic.PlayCard(EffectsUsed.Deploy, gm.currentFocusCardLogic.cardController);
                }
                gm.currentFocusCardLogic.RemoveFocusCardLogic();
            }
            //hold check to show card information
            else if (touchEndTime - touchStartTime > 0.5 && Vector2.Distance(touchEndPosition, touchStartPosition) < 3f)
            {
                if (!gm.currentFocusCardLogic.isFaceDown)
                    ShowEffectInfoPanel();
            }
            else
                if (playableLogic != null)
                playableLogic.EnableHover();
        }
        hasRaycast = false;
    }

    private void RaycastTargeting(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f))
            if (raycastHit.transform != null)
                CurrentClickedGameObject(raycastHit.transform.gameObject);
    }

    private void CurrentClickedGameObject(GameObject gameObject)
    {
        Debug.Log($"{gameObject.name} {gameObject.tag}");
        DisableAllPopups();
        if (isUsingshield)
            return;
        if (gameObject.CompareTag("Active UI panel"))
            return;
        if (gameObject.CompareTag("Background") && gm.gameState == GameState.Open)
        {
            audioManager.NewAudioPrefab(audioManager.unselect);
            if (gm.isChecking)
                DisableCardScrollScreen();
            if (toolTipManager.floatingInfoTexts.Count > 0)
            {
                toolTipManager.DisableInfoPauseMode();
                return;
            }
            if (gm.currentFocusCardLogic != null)
                gm.currentFocusCardLogic.RemoveFocusCardLogic();
            if (gm.isShowingLog)
            {
                if (!gameLogScreenRayBlocker.activeInHierarchy)
                    EnableLog();
                else
                {
                    if (scrollingLogPanelHandler.isShowingTargets)
                        scrollingLogPanelHandler.DisableTargetScroll();
                    else
                        ClearLogScrollScreen();
                }
            }
        }
        DisableDeckSearchButtons();
        if (gameObject.CompareTag("deck") && gm.gameState == GameState.Open && cardScrollScreen.activeInHierarchy == false)
        {
            //if you need this later for whatever reason, go ahead but it should be private knowledge in a normal game
            //if (gameObject == gm.RedPlayerManager.deck)
            //    gm.RedPlayerManager.deckSearchButton.SetActive(true);
            if (gameObject == gm.BluePlayerManager.deck)
                gm.BluePlayerManager.deckSearchButton.SetActive(true);
            if (gameObject == gm.RedPlayerManager.grave)
                gm.RedPlayerManager.graveSearchButton.SetActive(true);
            if (gameObject == gm.BluePlayerManager.grave)
                gm.BluePlayerManager.graveSearchButton.SetActive(true);
            return;
        }
        if (!gm.isChecking)
        {
            CardLogic focusCard = gm.currentFocusCardLogic;
            audioManager.NewAudioPrefab(audioManager.select);
            gameObject.TryGetComponent(out MonsterLogic monsterLogic);
            gameObject.TryGetComponent(out CombatantLogic combatant);
            if (gameObject.CompareTag("card"))
            {
                CardLogic clickedCard = gameObject.GetComponent<CardLogic>();
                if (gm.gameState == GameState.Open)
                {
                    if (clickedCard.currentLocation == Location.Deck || clickedCard.currentLocation == Location.HeroDeck)
                        return;
                    if (gm.isActivatingEffect || gm.isPlayingCard || !allowCardLogicSwap || gm.activationChainList.Count > 0)
                        return;
                    clickedCard.SetFocusCardLogic();
                    focusCard = gm.currentFocusCardLogic;
                    if (focusCard.type != Type.God)
                        focusCard.cardOutline.gameObject.SetActive(true);
                    if (focusCard.currentLocation == Location.Field && gm.turnPlayer == focusCard.cardController &&
                        focusCard.type != Type.God)
                    {
                        bool showButton = false;
                        if (focusCard.effects != null)
                            foreach (Effect effect in focusCard.effects)
                                foreach (SubEffect subEffect in effect.SubEffects)
                                    if (subEffect.effectType == EffectTypes.Deployed)
                                        if (effect.currentActivations < effect.maxActivations)
                                            showButton = true;
                        if (showButton)
                            monsterLogic.currentSlot.effectActivationButton.SetActive(true);
                        if (gm.currentPhase == Phase.BattlePhase)
                            if (combatant.attacksLeft > 0 && combatant.currentAtk > 0)
                                monsterLogic.currentSlot.attackDeclarationButton.SetActive(true);
                        return;
                    }
                }
                //targeting for effect
                else if (gm.gameState == GameState.Targeting && focusCard != null && clickedCard.currentLocation == Location.Field)
                    clickedCard.ManualTargetAcquisition(focusCard.focusSubEffect);

                //targeting for attack
                else if (gm.gameState == GameState.AttackDeclaration && focusCard != null && clickedCard.currentLocation == Location.Field)
                    combatant.AttackTargetAcquisition();
            }
        }
        DisableRayBlocker();
        DisableEffectInfoPanels();
        DisableEffectPanel();
        gm.EnableTurnUI();
    }

    public void DisableEffectInfoPanels() => infoPanel.SetActive(false);

    public void DisableDeckSearchButtons()
    {
        gm.RedPlayerManager.deckSearchButton.SetActive(false);
        gm.BluePlayerManager.deckSearchButton.SetActive(false);
        gm.RedPlayerManager.graveSearchButton.SetActive(false);
        gm.BluePlayerManager.graveSearchButton.SetActive(false);
    }

    public void ShowEffectInfoPanel()
    {
        string cost = "";
        gm.currentFocusCardLogic.TryGetComponent(out CombatantLogic combatantLogic);

        rayBlocker.SetActive(true);
        if (gm.currentFocusCardLogic.TryGetComponent(out PlayableLogic playableLogic))
            cost = playableLogic.cost.ToString();

        infoPanel.SetActive(true);
        infoPanelEnergy.SetActive(false);
        infoPanelStats.SetActive(false);

        infoPanelEffectText.text = gm.currentFocusCardLogic.cardText.Replace("|", System.Environment.NewLine);
        infoPanelFlavourText.text = gm.currentFocusCardLogic.flavorText;
        infoPanelNameText.text = gm.currentFocusCardLogic.cardName;
        infoPanelImage.sprite = gm.currentFocusCardLogic.image;

        if (gm.currentFocusCardLogic.playTypes.Contains(PlayType.Playable))
        {
            infoPanelEnergy.SetActive(true);
            infoPanelCostText.text = cost;
        }
        if (gm.currentFocusCardLogic.playTypes.Contains(PlayType.Combatant))
        {
            infoPanelStats.SetActive(true);
            infoPanelAtkText.text = combatantLogic.atk.ToString();
            infoPanelHpText.text = combatantLogic.hp.ToString();
            infoPanelStatusBar.SetActive(combatantLogic.cardStatuses.Count > 0);
            statScrollRayBlocker.SetActive(combatantLogic.cardStatuses.Count > 0);
            if (infoScrollingStatusPanelHandler != null)
                infoScrollingStatusPanelHandler.RemoveStatusImages();
            if (combatantLogic.cardStatuses.Count > 0)
                foreach (CardStatus cardStatus in combatantLogic.cardStatuses)
                    infoScrollingStatusPanelHandler.AddStatusImage(cardStatus);
        }
    }

    public void DeclareAttack()
    {
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().currentSlot.effectActivationButton.SetActive(false);
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().currentSlot.attackDeclarationButton.SetActive(false);
        gm.currentFocusCardLogic.GetComponent<CombatantLogic>().DeclareAttack();
    }

    public void ShowShieldPrompt(PlayerManager player)
    {
        isUsingshield = true;
        gm.DisableTurnUI();
        player.shieldPanel.SetActive(true);
        player.raycastBlocker.SetActive(true);
    }

    public void ShowEffectPanel()
    {
        allowCardLogicSwap = false;
        effectPanel.SetActive(true);
        rayBlocker.SetActive(true);
        effectPanelNameText.text = gm.currentFocusCardLogic.cardName;
        effectText = new List<string>(gm.currentFocusCardLogic.cardText.Split("|"));
        gm.DisableTurnUI();
        SwitchEffectPanel(0);
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().currentSlot.effectActivationButton.SetActive(false);
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().currentSlot.attackDeclarationButton.SetActive(false);
    }

    public void SwitchEffectPanel(int effectCount)
    {
        CardLogic activatingCard = gm.currentFocusCardLogic;
        Effect activatingEffect = activatingCard.effects[effectCount];
        SubEffect activatingSubEffect = activatingEffect.SubEffects[0];
        List<CardLogic> validTargets = new(activatingCard.GetValidTargets(activatingSubEffect));
        for (int i = 0; i < effectPanelTexts.Length; i++)
        {
            //only show buttons if its not the current effect
            if (i != effectCount)
            {
                effects[i].SetActive(false);
                activateButtons[i].SetActive(false);
                if (i >= effectText.Count)
                    switchButtons[i].SetActive(false);
                else
                    switchButtons[i].SetActive(true);
            }
        }
        effects[effectCount].SetActive(true);
        effectPanelTexts[effectCount].text = effectText[effectCount];
        switchButtons[effectCount].SetActive(false);
        if (activatingEffect.currentActivations >= activatingEffect.maxActivations || validTargets.Count == 0)
        {
            activateButtons[effectCount].SetActive(false);
            return;
        }
        //only show activation button if it's not a chain effect and in correct activation location
        if ((activatingEffect.activationLocations == null || activatingEffect.activationLocations.Contains(activatingCard.currentLocation)) 
            && activatingEffect.triggerLocations == null)
        {
            activateButtons[effectCount].SetActive(true);
            activateButtons[effectCount].GetComponentInChildren<TMP_Text>().text = (activatingCard.effects[effectCount].maxActivations
                - activatingCard.effects[effectCount].currentActivations).ToString();
        }
    }

    public void EffectActivation(int num)
    {
        gm.isActivatingEffect = true;
        gm.currentFocusCardLogic.EffectActivation(gm.currentFocusCardLogic.effects[num].SubEffects[0]);
        DisableEffectPanel();
    }

    private void DisableEffectPanel()
    {
        effectPanel.SetActive(false);
        allowCardLogicSwap = true;
    }

    public void ShieldActivation(PlayerManager player)
    {
        player.shieldPanel.SetActive(false);
        player.raycastBlocker.SetActive(false);
        gm.EnableTurnUI();
        isUsingshield = false;
        player.heroCardLogic.ActivateShield();
    }

    public void ShieldNotUsed(PlayerManager player)
    {
        player.shieldPanel.SetActive(false);
        player.raycastBlocker.SetActive(false);
        gm.EnableTurnUI();
        isUsingshield = false;
        player.heroCardLogic.ShieldPass();
    }

    public void EnableCardScrollScreen(List<CardLogic> cardLogics, bool shouldShowButton)
    {
        cardScrollScreen.SetActive(true);
        cardScrollRayBlocker.SetActive(true);
        cardScrollScreenButton.SetActive(shouldShowButton);
        scrollingCardPanelHandler.ClearScrollCardsList();
        scrollingCardPanelHandler.RemoveContentCards();
        scrollingCardPanelHandler.AddCardListToScrollCards(cardLogics);
        scrollingCardPanelHandler.AddContentCards();
        gm.isChecking = false;
    }

    public void DisableCardScrollScreen()
    {
        scrollingCardPanelHandler.ClearScrollCardsList();
        scrollingCardPanelHandler.RemoveContentCards();
        cardScrollScreen.SetActive(false);
        cardScrollRayBlocker.SetActive(false);
        gm.isChecking = false;
    }

    public void ShowCardList(GameObject gameObject)
    {
        cardScrollScreen.SetActive(true);
        cardScrollRayBlocker.SetActive(true);
        cardScrollScreenButton.SetActive(false);
        scrollingCardPanelHandler.ClearScrollCardsList();
        scrollingCardPanelHandler.RemoveContentCards();
        gm.isChecking = true;
        if (gameObject == gm.RedPlayerManager.deck)
        {
            scrollingCardPanelHandler.AddCardListToScrollCards(gm.RedPlayerManager.deckLogicList);
            scrollingCardPanelHandler.AddContentCards();
        }
        if (gameObject == gm.BluePlayerManager.deck)
        {
            scrollingCardPanelHandler.AddCardListToScrollCards(gm.BluePlayerManager.deckLogicList);
            scrollingCardPanelHandler.AddContentCards();
        }
        if (gameObject == gm.RedPlayerManager.grave)
        {
            scrollingCardPanelHandler.AddCardListToScrollCards(gm.RedPlayerManager.graveLogicList);
            scrollingCardPanelHandler.AddContentCards();
        }
        if (gameObject == gm.BluePlayerManager.grave)
        {
            scrollingCardPanelHandler.AddCardListToScrollCards(gm.BluePlayerManager.graveLogicList);
            scrollingCardPanelHandler.AddContentCards();
        }
        DisableDeckSearchButtons();
        return;
    }

    public void EnableLog()
    {
        gameLogScrollScreen.SetActive(true);
        gameLogScreenRayBlocker.SetActive(true);
        gameLogButton.SetActive(false);
        gm.isChecking = true;
        gm.isShowingLog = true;
        if (scrollingLogPanelHandler.isShowingTargets)
            scrollingLogPanelHandler.EnableTargetScroll();
    }

    public void DisableLog()
    {
        gameLogButton.SetActive(true);
        gameLogScrollScreen.SetActive(false);
        gameLogScreenRayBlocker.SetActive(false);
        gm.isChecking = false;
        if(scrollingLogPanelHandler.isShowingTargets)
            scrollingLogPanelHandler.DisableTargetScroll();
    }
    public void LoadLogScrollScreen()
    {
        EnableLog();
        scrollingLogPanelHandler.ClearScrollEntries();
        scrollingLogPanelHandler.RemoveContentLogs();
        scrollingLogPanelHandler.AddEntriesToScrollEntries(gm.gameLogHistoryEntries);
        scrollingLogPanelHandler.AddContentLogs();
    }

    public void ClearLogScrollScreen()
    {
        scrollingLogPanelHandler.ClearScrollEntries();
        scrollingLogPanelHandler.RemoveContentLogs();
        DisableLog();
        gm.isShowingLog = false;
    }

    public void EnableEffectActivationPanel()
    {
        effectActivationPanel.SetActive(true);
        gm.isActivatingEffect = true;
        rayBlocker.SetActive(true);
        effectActivationPanelText.text = $"{effectActivationText} {gm.currentFocusCardLogic.cardName}?";
        gm.DisableTurnUI();
    }

    public void OptionalEffectHandler(bool used)
    {
        gm.isActivatingEffect = used;
        gm.currentFocusCardLogic.OptionalEffectResolution(used);
        effectActivationPanel.SetActive(false);
        rayBlocker.SetActive(false);
        gm.EnableTurnUI();
    }

    public void ResolveOptionalTargeting()
    {
    gm.currentFocusCardLogic.EffectResolution(gm.currentFocusCardLogic.focusSubEffect);
        DisableCardScrollScreen();
    }

    public void GameOver(PlayerManager winner)
    {
        gameOverPanel.SetActive(true);
        gameOverWinnerText.text = winner.PlayerName + " wins";
    }

    public void DisableRayBlocker() => rayBlocker.SetActive(false);

    private static Vector3 ScreenToWorld(Vector3 position)
    {
        position.z = 25f;
        return Camera.main.ScreenToWorldPoint(position);
    }

    private void DisableAllPopups()
    {
        foreach (CardSlot slot in gm.Row1)
        {
            slot.attackDeclarationButton.SetActive(false);
            slot.effectActivationButton.SetActive(false);
        }
        foreach (CardSlot slot in gm.Row2)
        {
            slot.attackDeclarationButton.SetActive(false);
            slot.effectActivationButton.SetActive(false);
        }
        foreach (CardSlot slot in gm.Row3)
        {
            slot.attackDeclarationButton.SetActive(false);
            slot.effectActivationButton.SetActive(false);
        }
        foreach (CardSlot slot in gm.Row4)
        {
            slot.attackDeclarationButton.SetActive(false);
            slot.effectActivationButton.SetActive(false);
        }
    }
}

