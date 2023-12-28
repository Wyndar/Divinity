using UnityEngine;
using TMPro;
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
        //int odds = Random.Range(1, 101);
        //gm.popUpPanel.SetActive(true);
        //if (odds < 51)
        //{
            gm.popUpPanelText.text = "You're First";
            gm.popUpPanelText.color = playerColor;
            gm.turnPhaseText.color = playerColor;
            currentCoroutine = StartCoroutine(FirstTurn(gm.BluePlayerManager));
        //}
        //else
        //{
        //    gm.popUpPanelText.text = "Your Opponent goes first";
        //    gm.popUpPanelText.color= enemyColor; 
        //    gm.turnPhaseText.color= enemyColor;
        //    currentCoroutine = StartCoroutine(FirstTurn(gm.RedPlayerManager));
        //}
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
        gm.phaseChangeButtonText.text = "COMBAT";

        StartCoroutine(gm.DrawCard(5, gm.RedPlayerManager));
        StartCoroutine(gm.DrawCard(5, gm.BluePlayerManager));
        yield return new WaitUntil(()=>gm.hasFinishedDrawEffect == true);

        gm.isNotFirstDraw = true;
        gm.currentFocusCardLogic = null;
        gm.StateReset();
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
        yield return new WaitUntil(() => gm.hasFinishedDrawEffect == true);

        gm.currentFocusCardLogic = null;
        gm.AllEffectsRefresh(player);
        gm.AllAttacksRefresh(player);
        gm.AllCountdownReset();
        gm.ShieldRefresh(player);
        gm.StateReset();
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

        gm.currentFocusCardLogic = null;
        gm.StateChange(GameState.TurnEnd);
        gm.ChainResolution();
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        gm.currentFocusCardLogic = null;
        gm.AllTimersCountdown();
        gm.ChainResolution();
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        gm.currentFocusCardLogic = null;
        gm.StateReset();
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
        gm.currentFocusCardLogic = null;
        gm.StateReset();
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

        int amount = player.costPhaseGain;
        gm.CostChange(amount, player, true);
        gm.ChainResolution();
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        player.costPhaseGain++;
        gm.currentFocusCardLogic = null;
        gm.StateReset();
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
        gm.popUpPanelText.text = "deployment";
        gm.phaseChangeButton.SetActive(true);
        gm.phaseChangeButtonText.text = "COMBAT";
        PhaseButtonCheck(player);
        yield return new WaitForSeconds(waitTime);

        gm.PhaseChange(Phase.MainPhase);
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        gm.currentFocusCardLogic = null;
        gm.StateReset();
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
        gm.phaseChangeButton.SetActive(true);
        gm.phaseChangeButtonText.text = "END TURN";
        PhaseButtonCheck(player);
        yield return new WaitForSeconds(waitTime);

        Destroy(audioSource.gameObject);
        gm.PhaseChange(Phase.BattlePhase);
        yield return new WaitUntil(() => gm.activationChainList.Count == 0 && gm.gameState == GameState.Open);

        gm.currentFocusCardLogic = null;
        gm.StateReset();
        yield break;
    }

    public void TriggerPhaseChange()
    {
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

    private void PhaseButtonCheck(PlayerManager player)
    {
        if (player.isAI == false && player.isLocal == true)
            gm.phaseChangeButton.SetActive(true);
        else
            gm.phaseChangeButton.SetActive(false);
        if (player == gm.BluePlayerManager)
            gm.phaseChangeButton.transform.rotation = Quaternion.Euler(Vector3.zero);
        else
            gm.phaseChangeButton.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
    }
}

