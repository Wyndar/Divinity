using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class PrimaryUIManager : MonoBehaviour
{
    [SerializeField] private GameBattleManager gm;

    [SerializeField] private InputManager InputManager;

    [SerializeField] private AudioManager audioManager;

    [SerializeField] private ToolTipManager toolTipManager;

    [SerializeField] private ScrollingCardPanelHandler scrollingCardPanelHandler;

    [SerializeField] private ScrollingLogPanelHandler scrollingLogPanelHandler;

    [SerializeField] private ScrollingStatusPanelHandler infoScrollingStatusPanelHandler;

    [SerializeField] private GameObject effectPanel, infoPanelStatusBar, infoPanel, infoPanelStats, infoPanelEnergy,
        rayBlocker, cardScrollScreen, gameLogScrollScreen, effectActivationPanel, cardScrollScreenButton, gameLogScreenRayBlocker,
        cardScrollRayBlocker, statScrollRayBlocker, gameOverPanel, gameOverRayBlocker, gameLogButton;

    [SerializeField] private TMP_Text infoPanelNameText, infoPanelAtkText, infoPanelHpText, infoPanelCostText,
        infoPanelEffectText, infoPanelFlavourText, effectPanelNameText, effectActivationPanelText, gameOverWinnerText;

    [SerializeField] private Image infoPanelImage;

    [SerializeField] private TMP_Text[] effectPanelTexts;

    [SerializeField] private GameObject[] effects, switchButtons, activateButtons;

    [SerializeField] private string effectActivationText;

    [SerializeField] private LayerMask cardSlotLayer;

    public Vector2 touchStartPosition, touchEndPosition;
    private float touchStartTime, touchEndTime;
    private Coroutine moveCoroutine;
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
        if (gm.currentFocusCardLogic != null)    
            gm.currentFocusCardLogic.visualsLogic.ToggleCardOutline(false);
        touchStartPosition = gm.ScreenToWorld(screenPosition);
        touchStartTime = time;
        moveCoroutine = StartCoroutine(Move(screenPosition));
    }

    private IEnumerator Move(Vector2 screenPosition)
    {
        if (hasRaycast == false)
        {
            hasRaycast = true;
            RaycastTargeting(screenPosition);
        }

        while (rayBlocker.activeInHierarchy == false)
        {
            //card slot highlight
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(InputManager.CurrentFingerPosition.x,
                InputManager.CurrentFingerPosition.y, Camera.main.nearClipPlane));
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, cardSlotLayer))
            {
                if (gm.gameState == GameState.Moving)
                    gm.currentFocusCardLogic.GetComponent<MonsterLogic>().Move(raycastHit.transform.gameObject.GetComponent<CardSlot>());
                else
                    raycastHit.transform.gameObject.GetComponent<CardSlot>().SelectSlot();
            }
            else if (gm.currentFocusCardSlot != null)
                gm.currentFocusCardSlot.DeselectSlot();
            if (gm.currentFocusCardLogic != null)
                if (gm.currentFocusCardLogic.dataLogic.currentLocation == Location.Hand && gm.currentPhase == Phase.MainPhase &&
                    gm.turnPlayer == gm.currentFocusCardLogic.dataLogic.cardController)
                    gm.currentFocusCardLogic.transform.position = gm.ScreenToWorld(InputManager.CurrentFingerPosition);
            yield return null;
        }
    }

    private void TouchEnd(Vector2 screenPosition, float time)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        touchEndPosition = gm.ScreenToWorld(screenPosition);
        touchEndTime = time;
        if (gm.gameState != GameState.Open)
        {
            hasRaycast = false;
            return;
        }
        if (gm.currentFocusCardLogic != null)
        {
            if (gm.currentFocusCardLogic.dataLogic.currentLocation == Location.Hand)
                gm.currentFocusCardLogic.visualsLogic.transform.localPosition = Vector3.zero;
            //swipe check to play
            if (touchEndTime - touchStartTime > 0.1 && touchEndTime - touchStartTime < 1 &&
                Vector2.Distance(touchEndPosition, touchStartPosition) >= 0.05f &&
                gm.currentFocusCardLogic.dataLogic.currentLocation == Location.Hand)
            {
                float playDist = Mathf.Abs(touchEndPosition.x - touchStartPosition.x);
                gm.currentFocusCardLogic.visualsLogic.TryGetComponent(out PlayableLogic playableLogic);
                if (gm.currentPhase == Phase.MainPhase && playableLogic != null)
                {
                    if (gm.currentFocusCardLogic.dataLogic.type == Type.Spell && playDist > 2f)
                        playableLogic.PlayCard(EffectsUsed.Deploy, gm.currentFocusCardLogic.dataLogic.cardController);
                    if (gm.currentFocusCardSlot != null && gm.currentFocusCardLogic.dataLogic.type == Type.Fighter)
                        playableLogic.PlayCard(EffectsUsed.Deploy, gm.currentFocusCardLogic.dataLogic.cardController);
                }
            }
            //hold check to show card information
            else if (touchEndTime - touchStartTime > 0.5 && Vector2.Distance(touchEndPosition, touchStartPosition) < 1f &&
                !gm.currentFocusCardLogic.visualsLogic.isFaceDown)
                ShowEffectInfoPanel();
        }
        hasRaycast = false;
    }

    private void RaycastTargeting(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, ~cardSlotLayer))
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
        if (gameObject.CompareTag("GameOver"))
            gm.LoadScene(0);
        //the sprites are interfering with UI at times and it's annoying so we do this
        if (gameObject.CompareTag("Button"))
        {
            gameObject.GetComponent<Button>().onClick.Invoke();
            return;
        }
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
                    if (clickedCard.dataLogic.currentLocation == Location.Deck || clickedCard.dataLogic.currentLocation == Location.HeroDeck)
                        return;
                    if (gm.isActivatingEffect || gm.isPlayingCard || !allowCardLogicSwap || gm.activationChainList.Count > 0)
                        return;
                    clickedCard.SetFocusCardLogic();
                    focusCard = gm.currentFocusCardLogic;
                    if (focusCard.dataLogic.type != Type.God)
                        focusCard.visualsLogic.cardOutline.gameObject.SetActive(true);
                    else
                        focusCard.dataLogic.cardController.effectActivationButton.SetActive(true);
                    if (focusCard.dataLogic.currentLocation == Location.Field && gm.turnPlayer == focusCard.dataLogic.cardController &&
                        focusCard.dataLogic.type != Type.God)
                    {
                        monsterLogic.currentSlot.effectActivationButton.SetActive(focusCard.effectLogic.IsValidEffect());
                        if (!monsterLogic.hasMoved && gm.currentPhase == Phase.MainPhase)
                            monsterLogic.currentSlot.moveButton.SetActive(true);
                        if (gm.currentPhase == Phase.BattlePhase)
                            if (combatant.attacksLeft > 0 && combatant.currentAtk > 0 && combatant.GetValidAttackTargets().Count != 0)
                                monsterLogic.currentSlot.attackDeclarationButton.SetActive(true);
                        return;
                    }
                }
                //targeting for effect
                else if (gm.gameState == GameState.Targeting && focusCard != null && clickedCard.dataLogic.currentLocation == Location.Field)
                    clickedCard.targetingLogic.ManualTargetAcquisition(focusCard.effectLogic.focusSubEffect);

                //targeting for attack
                else if (gm.gameState == GameState.AttackDeclaration && focusCard != null && clickedCard.dataLogic.currentLocation == Location.Field)
                    combatant.AttackTargetAcquisition();
            }
        }
        DisableRayBlocker();
        DisableEffectInfoPanels();
        DisableEffectPanel();
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
        gm.currentFocusCardLogic.visualsLogic.TryGetComponent(out CombatantLogic combatantLogic);

        rayBlocker.SetActive(true);
        if (gm.currentFocusCardLogic.visualsLogic.TryGetComponent(out PlayableLogic playableLogic))
            cost = playableLogic.cost.ToString();

        infoPanel.SetActive(true);
        infoPanelEnergy.SetActive(false);
        infoPanelStats.SetActive(false);
        infoPanelStatusBar.SetActive(false);

        infoPanelEffectText.text = gm.currentFocusCardLogic.dataLogic.cardText.Replace("|", System.Environment.NewLine);
        infoPanelFlavourText.text = gm.currentFocusCardLogic.dataLogic.flavorText;
        infoPanelNameText.text = gm.currentFocusCardLogic.dataLogic.cardName;
        infoPanelImage.sprite = gm.currentFocusCardLogic.visualsLogic.image;

        if (gm.currentFocusCardLogic.dataLogic.playTypes.Contains(PlayType.Playable))
        {
            infoPanelEnergy.SetActive(true);
            infoPanelCostText.text = cost;
        }
        if (gm.currentFocusCardLogic.dataLogic.playTypes.Contains(PlayType.Combatant))
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
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().currentSlot.moveButton.SetActive(false);
        gm.currentFocusCardLogic.GetComponent<CombatantLogic>().DeclareAttack();
    }

    public void DeclareMove()
    {
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().currentSlot.effectActivationButton.SetActive(false);
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().currentSlot.attackDeclarationButton.SetActive(false);
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().currentSlot.moveButton.SetActive(false);
        gm.currentFocusCardLogic.GetComponent<MonsterLogic>().DeclareMove();
    }

    public void ShowShieldPrompt(PlayerManager player)
    {
        isUsingshield = true;
        player.shieldPanel.SetActive(true);
        player.raycastBlocker.SetActive(true);
    }

    public void ShowEffectPanel()
    {
        allowCardLogicSwap = false;
        effectPanel.SetActive(true);
        rayBlocker.SetActive(true);
        effectPanelNameText.text = gm.currentFocusCardLogic.dataLogic.cardName;
        effectText = new List<string>(gm.currentFocusCardLogic.dataLogic.cardText.Split("|"));
        SwitchEffectPanel(0);
        if (gm.currentFocusCardLogic is MonsterLogic monster)
        {
            monster.currentSlot.effectActivationButton.SetActive(false);
            monster.currentSlot.moveButton.SetActive(false);
            monster.currentSlot.attackDeclarationButton.SetActive(false);
        }
        else
            gm.currentFocusCardLogic.dataLogic.cardController.effectActivationButton.SetActive(false);
    }

    public void SwitchEffectPanel(int effectCount)
    {
        CardLogic activatingCard = gm.currentFocusCardLogic;
        Effect activatingEffect = activatingCard.effectLogic.effects[effectCount];
        SubEffect activatingSubEffect = activatingEffect.SubEffects[0];
        List<CardLogic> validTargets = new(activatingCard.targetingLogic.GetValidTargets(activatingSubEffect, false));
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
        if (activatingEffect.currentActivations >= activatingEffect.maxActivations || 
            (activatingSubEffect.effectUsed != EffectsUsed.BloodCost && validTargets.Count == 0) ||
             (activatingSubEffect.effectUsed == EffectsUsed.BloodCost && 
             activatingCard.dataLogic.cardController.BloodAttunementCheck(Enum.Parse<Attunement>( activatingSubEffect.TargetStats[0]))
             < activatingSubEffect.effectAmount))
        {
            activateButtons[effectCount].SetActive(false);
            return;
        }
        //only show activation button if it's not a chain effect and in correct activation location
        if ((activatingEffect.activationLocations == null || activatingEffect.activationLocations.Contains(activatingCard.dataLogic.currentLocation)) 
            && activatingEffect.triggerLocations == null)
        {
            activateButtons[effectCount].SetActive(true);
            if (activatingCard.effectLogic.effects[effectCount].maxActivations >= 98)
                activateButtons[effectCount].GetComponentInChildren<TMP_Text>().text = "";
            else
                activateButtons[effectCount].GetComponentInChildren<TMP_Text>().text = (activatingCard.effectLogic.effects[effectCount].maxActivations
                    - activatingCard.effectLogic.effects[effectCount].currentActivations).ToString();
        }
    }

    public void EffectActivation(int num)
    {
        gm.isActivatingEffect = true;
        gm.currentFocusCardLogic.EffectActivation(gm.currentFocusCardLogic.effectLogic.effects[num].SubEffects[0]);
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
        isUsingshield = false;
        player.heroCardLogic.ActivateShield();
    }

    public void ShieldNotUsed(PlayerManager player)
    {
        player.shieldPanel.SetActive(false);
        player.raycastBlocker.SetActive(false);
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
        effectActivationPanelText.text = $"{effectActivationText} {gm.currentFocusCardLogic.dataLogic.cardName}?";
    }

    public void OptionalEffectHandler(bool used)
    {
        gm.isActivatingEffect = used;
        gm.currentFocusCardLogic.effectLogic.OptionalEffectResolution(used);
        effectActivationPanel.SetActive(false);
        rayBlocker.SetActive(false);
    }

    public void ResolveOptionalTargeting()
    {
    gm.currentFocusCardLogic.EffectResolution(gm.currentFocusCardLogic.effectLogic.focusSubEffect);
        DisableCardScrollScreen();
    }

    public void GameOver(PlayerManager winner)
    {
        gameOverPanel.SetActive(true);
        gameOverRayBlocker.SetActive(true);
        gameOverWinnerText.text = winner.PlayerName + " wins";
    }

    public void DisableRayBlocker() => rayBlocker.SetActive(false);

    

    private void DisableAllPopups()
    {
        foreach (CardSlot slot in gm.Row1)
        {
            slot.attackDeclarationButton.SetActive(false);
            slot.moveButton.SetActive(false);
            slot.effectActivationButton.SetActive(false);
        }
        foreach (CardSlot slot in gm.Row2)
        {
            slot.attackDeclarationButton.SetActive(false);
            slot.moveButton.SetActive(false);
            slot.effectActivationButton.SetActive(false);
        }
        foreach (CardSlot slot in gm.Row3)
        {
            slot.attackDeclarationButton.SetActive(false);
            slot.moveButton.SetActive(false);
            slot.effectActivationButton.SetActive(false);
        }
        foreach (CardSlot slot in gm.Row4)
        {
            slot.attackDeclarationButton.SetActive(false);
            slot.moveButton.SetActive(false);
            slot.effectActivationButton.SetActive(false);
        }
        gm.BluePlayerManager.effectActivationButton.SetActive(false);
        gm.RedPlayerManager.effectActivationButton.SetActive(false);
    }
}

