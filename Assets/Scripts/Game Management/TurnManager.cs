using UnityEngine;
using TMPro;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public Game_Manager gm;

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
            currentCoroutine = StartCoroutine(FirstTurn(gm.BluePlayerManager));
        }
        else
        {
            gm.popUpPanelText.text = "Your Opponent goes first";
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
        gm.DrawCard(5, gm.RedPlayerManager);
        gm.DrawCard(5, gm.BluePlayerManager);
        gm.turnCount = 1;
        gm.bluePlayerText.text = gm.BluePlayerManager.PlayerName;
        gm.redPlayerText.text = gm.RedPlayerManager.PlayerName;
        gm.turnCountText.text = gm.turnCount.ToString();
        gm.phaseChangeButtonText.text = "COMBAT";
        gm.currentFocusCardLogic = null;
        gm.isNotLoading = true;
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
        gm.DrawCard(1, player);
        gm.currentFocusCardLogic = null;
        gm.StateReset();
        gm.AllEffectsRefresh(player);
        gm.AllAttacksRefresh(player);
        gm.AllCountdownReset();
        gm.ShieldRefresh(player);
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
        gm.ChainResolution();
        gm.currentFocusCardLogic = null;
        gm.StateChange(GameState.TurnEnd);
        gm.AllTimersCountdown();
        gm.StateReset();
        gm.popUpPanel.SetActive(true);
        if (player == gm.BluePlayerManager)
        {
            gm.popUpPanelText.text = $"It's {gm.RedPlayerManager.PlayerName}'s turn";
            yield return new WaitForSeconds(waitTime);
            SwitchTurn(gm.RedPlayerManager);
        }
        else
        {
            gm.popUpPanelText.text = "It's your turn";
            yield return new WaitForSeconds(waitTime);
            SwitchTurn(gm.BluePlayerManager);
        }
        yield break;
    }

    public void SwitchTurn(PlayerManager player)
    {
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
        int amount = player.costPhaseGain;
        gm.CostChange(amount, player, true);
        player.costPhaseGain++;
        gm.currentFocusCardLogic = null;
        gm.StateChange(GameState.Cost);
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
        PhaseButtonCheck(player);
        yield return new WaitForSeconds(waitTime);

        gm.PhaseChange(Phase.MainPhase);
        gm.currentFocusCardLogic = null;
        gm.StateReset();
        yield break;
    }

    public IEnumerator BattlePhase(PlayerManager player)
    {
        StopCoroutine(previousCoroutine);
        previousCoroutine = currentCoroutine;
        currentCoroutine = null;
        gm.popUpPanel.SetActive(true);
        gm.turnPhaseText.text = "Combat";
        gm.popUpPanelText.text = "combat";
        PhaseButtonCheck(player);
        yield return new WaitForSeconds(waitTime);

        gm.PhaseChange(Phase.BattlePhase);
        gm.currentFocusCardLogic = null;
        gm.StateReset();
        yield break;
    }

    public void TriggerPhaseChange()
    {
        if (gm.currentPhase == Phase.MainPhase)
        {
            gm.phaseChangeButtonText.text = "END TURN";
            currentCoroutine = StartCoroutine(BattlePhase(gm.turnPlayer));
        }
        else if (gm.currentPhase == Phase.BattlePhase)
        {
            gm.phaseChangeButtonText.text = "COMBAT";
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

