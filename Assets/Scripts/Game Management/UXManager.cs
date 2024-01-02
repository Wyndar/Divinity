using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UXManager : MonoBehaviour
{
    [SerializeField]
    private Game_Manager gm;

    [SerializeField]
    private InputManager InputManager;

    [SerializeField]
    private EnumManager enumConverter;

    [SerializeField]
    private AudioManager audioManager;

    [SerializeField]
    private ScrollingCardPanelHandler scrollingCardPanelHandler;

    [SerializeField]
    private ScrollingLogPanelHandler scrollingLogPanelHandler;

    [SerializeField]
    private ScrollingStatusPanelHandler monsterScrollingStatusPanelHandler, godScrollingStatusPanelHandler;

    [SerializeField]
    private GameObject trail, effectPanel, infoPanelMonsterStatusBar, infoPanelGodStatusBar, infoPanelMonster, infoPanelSpell, infoPanelGod, rayBlocker, cardScrollScreen, gameLogScrollScreen, effectActivationPanel, cardScrollScreenButton, gameLogScreenButton, cardScrollRayBlocker, statScrollRayBlocker, gameOverPanel;

    [SerializeField]
    private TMP_Text infoPanelSpellNameText, infoPanelSpellCostText, infoPanelSpellEffectText, infoPanelSpellFlavourText,
    infoPanelMonsterNameText, infoPanelMonsterAtkText, infoPanelMonsterHpText, infoPanelMonsterCostText,
    infoPanelMonsterEffectText, infoPanelMonsterFlavourText, infoPanelGodNameText, infoPanelGodAtkText,
    infoPanelGodHpText, infoPanelGodEffectText, infoPanelGodFlavourText, effectPanelNameText,
    effectActivationPanelText, gameOverWinnerText;

    [SerializeField]
    private TMP_Text[] effectPanelTexts;

    [SerializeField]
    private GameObject[] effects, switchButtons, activateButtons;

    [SerializeField]
    private string effectActivationText;

    public Vector2 touchStartPosition, touchEndPosition;
    private float touchStartTime, touchEndTime;
    private Coroutine trailCoroutine;
    private List<string> effectText;
    private int activeEffectButton, activeAttackButton;
    private PlayerManager effectActivatingPlayer, attackDeclaringPlayer;
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
            gm.currentFocusCardLogic.cardOutline.gameObject.SetActive(false);
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
                float playDist = Mathf.Abs(touchEndPosition.y - touchStartPosition.y);
                if (playDist > 3f && gm.currentPhase == Phase.MainPhase && playableLogic != null)
                {
                    playableLogic.DisableHover();
                    playableLogic.PlayCard(EffectsUsed.Deploy, gm.currentFocusCardLogic.cardController);
                }
                gm.currentFocusCardLogic.cardOutline.gameObject.SetActive(false);
                gm.currentFocusCardLogic = null;
            }
            //hold check to show card information
            else if (touchEndTime - touchStartTime > 0.5 && Vector2.Distance(touchEndPosition, touchStartPosition) < 3f)
            {
                if(!gm.currentFocusCardLogic.isFaceDown)
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
        if (gm.isShowingInfo)
            return;
        if (gameObject.CompareTag("Active UI panel"))
            return;
        if (gameObject.CompareTag("Background") && gm.gameState == GameState.Open)
            gm.currentFocusCardLogic = null;
        DisableDeckSearchButtons();
        if (gameObject.CompareTag("deck") && gm.gameState == GameState.Open && cardScrollScreen.activeInHierarchy==false)
        {
            if (gameObject == gm.RedPlayerManager.deck)
                gm.RedPlayerManager.deckSearchButton.SetActive(true);
            if (gameObject == gm.BluePlayerManager.deck)
                gm.BluePlayerManager.deckSearchButton.SetActive(true);
            if (gameObject == gm.RedPlayerManager.grave)
                gm.RedPlayerManager.graveSearchButton.SetActive(true);
            if (gameObject == gm.BluePlayerManager.grave)
                gm.BluePlayerManager.graveSearchButton.SetActive(true);
            return;
        }
        CardLogic focusCard = gm.currentFocusCardLogic;
        audioManager.NewAudioPrefab(audioManager.select);
        gameObject.TryGetComponent(out MonsterLogic monsterLogic);
        gameObject.TryGetComponent(out CombatantLogic combatant);
        if (gameObject.CompareTag("card"))
        {
            CardLogic clickedCard = gameObject.GetComponent<CardLogic>();
            if (gm.gameState == GameState.Open)
            {
                if (clickedCard.currentLocation == Location.Deck)
                    return;
                if (clickedCard.currentLocation == Location.HeroDeck)
                    return;
                if (gm.isActivatingEffect)
                    return;
                if (gm.isPlayingCard)
                    return;
                if (!allowCardLogicSwap)
                    return;
                gm.currentFocusCardLogic = clickedCard;
                focusCard = gm.currentFocusCardLogic;
                focusCard.cardOutline.gameObject.SetActive(true);
                if (focusCard.currentLocation == Location.Field && gm.turnPlayer == focusCard.cardController && focusCard.cardType != "god")
                {
                    bool showButton = false;
                    if (focusCard.effects != null)
                        foreach (Effect effect in focusCard.effects)
                            foreach (EffectTypes effectTypes in effect.effectTypes)
                                if (effectTypes == EffectTypes.Deployed)
                                    if(effect.currentActivations < effect.maxActivations)
                                        showButton = true;
                    if (showButton)
                        EnableOnFieldEffectActivationPopupButton(monsterLogic.cardController, monsterLogic.locationOrderNumber);
                    if (gm.currentPhase == Phase.BattlePhase)
                        if (combatant.attacksLeft > 0 && combatant.currentAtk > 0)
                            EnableOnFieldAttackPopupButton(monsterLogic.cardController, monsterLogic.locationOrderNumber);
                    return;
                }
            }
            //targeting for effect
            else if (gm.gameState == GameState.Targeting && focusCard != null && clickedCard.currentLocation == Location.Field)
                clickedCard.ManualTargetAcquisition(focusCard.effectCountNumber, focusCard.subCountNumber);

            //targeting for attack
            else if (gm.gameState == GameState.AttackDeclaration && focusCard != null && clickedCard.currentLocation == Location.Field)
                combatant.AttackTargetAcquisition();
        }
        cardScrollRayBlocker.SetActive(false);
        DisableRayBlocker();
        DisableEffectInfoPanels();
        DisableEffectPanel();
        if (gm.isChecking)
            DisableCardScrollScreen();
        gm.EnableTurnUI();
    }

    public void DisableEffectInfoPanels()
    {
        infoPanelSpell.SetActive(false);
        infoPanelMonster.SetActive(false);
        infoPanelGod.SetActive(false);
    }

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
        gm.DisableTurnUI();
        if (gm.currentFocusCardLogic.TryGetComponent(out PlayableLogic playableLogic))
            cost = playableLogic.cost.ToString();
        
        switch (gm.currentFocusCardLogic.cardType)
        {
            case "spell":
                infoPanelSpell.SetActive(true);
                infoPanelSpellCostText.text = cost;
                infoPanelSpellEffectText.text = gm.currentFocusCardLogic.cardText.Replace("|", System.Environment.NewLine);
                infoPanelSpellFlavourText.text = gm.currentFocusCardLogic.flavorText;
                infoPanelSpellNameText.text = gm.currentFocusCardLogic.cardName;
                break;
            case "monster":
                infoPanelMonster.SetActive(true);
                infoPanelMonsterAtkText.text = combatantLogic.currentAtk.ToString();
                infoPanelMonsterCostText.text = cost;
                infoPanelMonsterHpText.text = combatantLogic.currentHp.ToString();
                infoPanelMonsterEffectText.text = gm.currentFocusCardLogic.cardText.Replace("|", System.Environment.NewLine);
                infoPanelMonsterFlavourText.text = gm.currentFocusCardLogic.flavorText;
                infoPanelMonsterNameText.text = gm.currentFocusCardLogic.cardName;
                infoPanelMonsterStatusBar.SetActive(combatantLogic.cardStatuses.Count > 0);
                statScrollRayBlocker.SetActive(combatantLogic.cardStatuses.Count > 0);
                monsterScrollingStatusPanelHandler.RemoveStatusImages();
                if (combatantLogic.cardStatuses.Count > 0)
                    foreach (CardStatus cardStatus in combatantLogic.cardStatuses)
                        monsterScrollingStatusPanelHandler.AddStatusImage(cardStatus);
                break;
            case "god":
                infoPanelGod.SetActive(true);
                infoPanelGodAtkText.text = combatantLogic.currentAtk.ToString();
                infoPanelGodHpText.text = combatantLogic.currentHp.ToString();
                infoPanelGodEffectText.text = gm.currentFocusCardLogic.cardText.Replace("|", System.Environment.NewLine);
                infoPanelGodFlavourText.text = gm.currentFocusCardLogic.flavorText;
                infoPanelGodNameText.text = gm.currentFocusCardLogic.cardName;
                infoPanelGodStatusBar.SetActive(combatantLogic.cardStatuses.Count > 0);
                statScrollRayBlocker.SetActive(combatantLogic.cardStatuses.Count > 0);
                godScrollingStatusPanelHandler.RemoveStatusImages();
                if (combatantLogic.cardStatuses.Count > 0)
                    foreach (CardStatus cardStatus in combatantLogic.cardStatuses)
                        godScrollingStatusPanelHandler.AddStatusImage(cardStatus);
                break;
        }
    }

    public void DeclareAttack()
    {
        if (effectActivatingPlayer != null)
            DisableOnFieldEffectActivationPopupButton(effectActivatingPlayer, activeEffectButton);
        DisableOnFieldAttackPopupButton(attackDeclaringPlayer, activeAttackButton); gm.currentFocusCardLogic.GetComponent<CombatantLogic>().DeclareAttack();
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
        DisableOnFieldEffectActivationPopupButton(effectActivatingPlayer, activeEffectButton);
        if (attackDeclaringPlayer != null)
            DisableOnFieldAttackPopupButton(attackDeclaringPlayer, activeAttackButton);
    }

    public void SwitchEffectPanel(int effectCount)
    {
        CardLogic activatingCard = gm.currentFocusCardLogic;
        Effect activatingEffect = activatingCard.effects[effectCount];
        List<CardLogic>validTargets = new(activatingCard.GetValidTargets(effectCount,0));
        for (int i = 0; i < effectPanelTexts.Length; i++)
        {
            //only show buttons if its not the current effect
            if (i != effectCount)
            {
                effects[i].SetActive(false);
                activateButtons[i].SetActive(false);
                if (i >= effectText.Count )
                    switchButtons[i].SetActive(false);
                else
                    switchButtons[i].SetActive(true);
            }
        }
        effects[effectCount].SetActive(true);
        effectPanelTexts[effectCount].text = effectText[effectCount];
        switchButtons[effectCount].SetActive(false);
        if (activatingEffect.ActivationLocation == null || activatingCard.effects[effectCount].currentActivations >= activatingCard.effects[effectCount].maxActivations || validTargets.Count == 0)
        {
            activateButtons[effectCount].SetActive(false);
            return;
        }
        //only show activation button if it's not a chain effect and in correct activation location
        if (activatingCard.currentLocation == enumConverter.LocationStringToEnum(activatingEffect.ActivationLocation) && activatingEffect.effectTypes[effectCount] != EffectTypes.Chain)
        {
            activateButtons[effectCount].SetActive(true);
            activateButtons[effectCount].GetComponentInChildren<TMP_Text>().text = (activatingCard.effects[effectCount].maxActivations - activatingCard.effects[effectCount].currentActivations).ToString();
        }
    }

    public void EffectActivation(int num)
    {
        gm.isActivatingEffect = true;
        gm.currentFocusCardLogic.EffectActivation(num, 0);
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

    public void EnableLogScrollScreen(List<CardLogic> cardLogics, bool shouldShowButton)
    {
        gameLogScrollScreen.SetActive(true);
        cardScrollRayBlocker.SetActive(true);
        gameLogScreenButton.SetActive(shouldShowButton);
        scrollingLogPanelHandler.ClearScrollCardsList();
        scrollingLogPanelHandler.RemoveContentCards();
        scrollingCardPanelHandler.AddCardListToScrollCards(cardLogics);
        scrollingCardPanelHandler.AddContentCards();
        gm.isChecking = false;
    }

    public void DisableLogScrollScreen()
    {
        scrollingLogPanelHandler.ClearScrollCardsList();
        scrollingLogPanelHandler.RemoveContentCards();
        gameLogScrollScreen.SetActive(false);
        cardScrollRayBlocker.SetActive(false);
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
    gm.currentFocusCardLogic.EffectResolution(gm.currentFocusCardLogic.effectCountNumber, gm.currentFocusCardLogic.subCountNumber);
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
    private void EnableOnFieldEffectActivationPopupButton(PlayerManager playerManager, int locationOrderNumber)
    {
        playerManager.effectActivationButtons[locationOrderNumber].SetActive(true);
        activeEffectButton = locationOrderNumber;
        effectActivatingPlayer = playerManager;
    }
    private void EnableOnFieldAttackPopupButton(PlayerManager playerManager, int locationOrderNumber)
    {
        playerManager.attackDeclarationButtons[locationOrderNumber].SetActive(true);
        activeAttackButton = locationOrderNumber;
        attackDeclaringPlayer = playerManager;
    }
    private void DisableOnFieldEffectActivationPopupButton(PlayerManager playerManager, int locationOrderNumber)
    {
        playerManager.effectActivationButtons[locationOrderNumber].SetActive(false);
        effectActivatingPlayer = null;
    }
    private void DisableOnFieldAttackPopupButton(PlayerManager playerManager, int locationOrderNumber)
    {
        playerManager.attackDeclarationButtons[locationOrderNumber].SetActive(false);
        attackDeclaringPlayer = null;
    }

    private void DisableAllPopups()
    {
        for(int i = 0; i<5;i++)
        {
            DisableOnFieldAttackPopupButton(gm.BluePlayerManager, i);
            DisableOnFieldEffectActivationPopupButton(gm.BluePlayerManager, i);
            DisableOnFieldAttackPopupButton(gm.RedPlayerManager, i);
            DisableOnFieldEffectActivationPopupButton(gm.RedPlayerManager, i);
        }
    }
}

