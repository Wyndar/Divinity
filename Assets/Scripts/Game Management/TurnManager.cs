using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public Game_Manager gm;
    public AudioManager audioManager;
    public Color playerColor;
    public Color enemyColor;
    public float waitTime = 2f;

    private Coroutine previousCoroutine;
    private Coroutine currentCoroutine;

    public IEnumerator ChooseFirstPlayer()
    {
        int odds = Random.Range(1, 101);
        gm.popUpPanel.SetActive(true);
        if (odds < 51)
        {
            gm.popUpPanelText.text = "You're First";
            gm.popUpPanelText.color = playerColor;
            gm.turnPhaseText.color = playerColor;
            currentCoroutine = StartCoroutine(FirstTurn(gm.BluePlayerManager));
        }
        else
        {
            gm.popUpPanelText.text = "Your Opponent goes first";
            gm.popUpPanelText.color = enemyColor;
            gm.turnPhaseText.color = enemyColor;
            currentCoroutine = StartCoroutine(FirstTurn(gm.RedPlayerManager));
        }
        yield break;
    }

    public IEnumerator FirstTurn(PlayerManager player)
    {
        StopCoroutine(ChooseFirstPlayer());
        previousCoroutine = currentCoroutine;

        yield return new WaitForSeconds(waitTime);

        gm.turnPlayer = player;
        gm.turnOpponent = player.enemy;
        gm.turnCount = 1;
        gm.bluePlayerText.text = gm.BluePlayerManager.PlayerName;
        gm.redPlayerText.text = gm.RedPlayerManager.PlayerName;
        gm.turnCountText.text = gm.turnCount.ToString();
        gm.BluePlayerManager.playerColor = playerColor;
        gm.RedPlayerManager.playerColor = enemyColor;
        gm.phaseChangeButtonText.text = "COMBAT";

        StartCoroutine(gm.DrawCard(5, gm.RedPlayerManager));
        StartCoroutine(gm.DrawCard(5, gm.BluePlayerManager));
        yield return new WaitUntil(()=>gm.hasFinishedDrawEffect == true);

        gm.isNotFirstDraw = true;
        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        gm.ChainResolution();
        currentCoroutine = StartCoroutine(CostPhase(player));
        yield break;
    }

    public IEnumerator DrawPhase(PlayerManager player)
    {
        StopCoroutine(previousCoroutine);
        previousCoroutine = currentCoroutine;
        gm.popUpPanel.SetActive(true);
        gm.turnPhaseText.text = "Reinforcement";
        gm.popUpPanelText.text = "Reinforcement";
        gm.phaseChangeButton.SetActive(false);
        gm.SwitchControl(player);

        yield return new WaitForSeconds(waitTime);

        gm.PhaseChange(Phase.DrawPhase);
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);
        StartCoroutine(gm.DrawCard(1, player));
        yield return new WaitUntil(() => gm.hasFinishedDrawEffect == true && gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        gm.AllEffectsRefresh(player);
        gm.AllAttacksRefresh(player);
        gm.AllCountdownReset();
        gm.ShieldRefresh(player);
        gm.ChainResolution();
        currentCoroutine = StartCoroutine(CostPhase(player));
        yield break;
    }

    public IEnumerator EndPhase(PlayerManager player)
    {
        StopCoroutine(previousCoroutine);
        previousCoroutine = currentCoroutine;
        currentCoroutine = null;
        gm.popUpPanel.SetActive(true);
        gm.turnPhaseText.text = "Retreat";
        gm.popUpPanelText.text = "Retreat";

        yield return new WaitForSeconds(waitTime);

        gm.PhaseChange(Phase.EndPhase);
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        gm.StateChange(GameState.TurnEnd);
        gm.ChainResolution();
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        gm.AllTimersCountdown();
        gm.ChainResolution();
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        gm.ChainResolution();
        gm.popUpPanel.SetActive(true);
        if (player == gm.BluePlayerManager)
        {
            gm.popUpPanelText.text = $"It's {gm.RedPlayerManager.PlayerName}'s turn";
            gm.popUpPanelText.color = enemyColor;
            gm.turnPhaseText.color = enemyColor;
            yield return new WaitForSeconds(waitTime);
            SwitchTurn(gm.RedPlayerManager);
        }
        else
        {
            gm.popUpPanelText.text = "It's your turn";
            gm.popUpPanelText.color= playerColor;
            gm.turnPhaseText.color = playerColor;
            yield return new WaitForSeconds(waitTime);
            SwitchTurn(gm.BluePlayerManager);
        }
        yield break;
    }

    public void SwitchTurn(PlayerManager player)
    {
        audioManager.NewAudioPrefab(audioManager.passTurn);
        gm.turnPlayer = player;
        gm.turnOpponent = player.enemy;
        gm.turnCount++;
        gm.turnCountText.text = gm.turnCount.ToString();
        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        gm.ChainResolution();
        currentCoroutine = StartCoroutine(DrawPhase(player));
    }

    public IEnumerator CostPhase(PlayerManager player)
    {
        if (previousCoroutine != null)
            StopCoroutine(previousCoroutine);
        previousCoroutine = currentCoroutine;
        gm.popUpPanel.SetActive(true);
        gm.turnPhaseText.text = "Recovery";
        gm.popUpPanelText.text = "Recovery";
        yield return new WaitForSeconds(waitTime);

        gm.PhaseChange(Phase.CostPhase);
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        if (player.costCount > 0)
            player.BloodAttunement(Attunement.Undefined, player.BloodAttunementCheck(Attunement.Untuned));
        player.BloodGain(Attunement.Untuned, player.costPhaseGain);
        gm.ChainResolution();
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        player.costPhaseGain++;
        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        gm.ChainResolution();
        currentCoroutine = StartCoroutine(MainPhase(player));
        yield break;
    }

    public IEnumerator MainPhase(PlayerManager player)
    {
        StopCoroutine(previousCoroutine);
        previousCoroutine = currentCoroutine; 
        currentCoroutine = null;
        gm.popUpPanel.SetActive(true);
        gm.turnPhaseText.text = "Deployment";
        gm.popUpPanelText.text = "Deployment";
        gm.phaseChangeButton.SetActive(true);
        gm.phaseChangeButtonText.text = "COMBAT";
        PhaseButtonCheck(player);
        yield return new WaitForSeconds(waitTime);

        gm.PhaseChange(Phase.MainPhase);
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        gm.ChainResolution();
        yield break;
    }

    public IEnumerator BattlePhase(PlayerManager player)
    {
        StopCoroutine(previousCoroutine);
        previousCoroutine = currentCoroutine;
        currentCoroutine = null;
        AudioSource audioSource = audioManager.NewAudioPrefab(audioManager.battlePhase);
        audioManager.BattlePhaseMusic(player.enemy.heroCardLogic.combatantLogic.currentHp < player.enemy.heroCardLogic.combatantLogic.maxHp / 4);
        gm.popUpPanel.SetActive(true);
        gm.turnPhaseText.text = "Combat";
        gm.popUpPanelText.text = "combat";
        yield return new WaitForSeconds(waitTime);

        audioSource.Stop();
        gm.PhaseChange(Phase.BattlePhase);
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        if (gm.currentFocusCardLogic != null)
            gm.currentFocusCardLogic.RemoveFocusCardLogic();
        PhaseButtonCheck(player);
        gm.phaseChangeButtonText.text = "END TURN";
        gm.ChainResolution();
        yield break;
    }

    public void TriggerPhaseChange()
    {
        if (gm.activationChainList.Count > 0)
            return;
        if (gm.gameState != GameState.Open)
            return;
        if (gm.currentPhase == Phase.MainPhase)
        {
            gm.phaseChangeButton.SetActive(false);
            gm.ClearEffectTargetImages();
            currentCoroutine = StartCoroutine(BattlePhase(gm.turnPlayer));
        }
        else if (gm.currentPhase == Phase.BattlePhase)
        {
            gm.phaseChangeButton.SetActive(false);
            audioManager.EndBattlePhaseMusic();
            gm.ClearEffectTargetImages();
            gm.ClearAttackTargetImages();
            currentCoroutine = StartCoroutine(EndPhase(gm.turnPlayer));
        }
    }

    private void PhaseButtonCheck(PlayerManager player)=>
        gm.phaseChangeButton.SetActive(!player.isAI && player.isLocal);
}

