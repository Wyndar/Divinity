using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
	public Game_Manager gm;

    public void ChooseFirstPlayer()
    {
        int odds = Random.Range(1, 101);
        if (odds < 51)
            FirstTurn(gm.BluePlayerManager);
        else
            FirstTurn(gm.RedPlayerManager);
    }

    public void FirstTurn(PlayerManager player)
    {
        gm.turnPlayer = player;
        gm.DrawCard(5, gm.RedPlayerManager);
        gm.DrawCard(5, gm.BluePlayerManager);
        gm.turnCount = 1;
        gm.bluePlayerText.text = gm.BluePlayerManager.PlayerName;
        gm.redPlayerText.text = gm.RedPlayerManager.PlayerName;
        gm.turnCountText.text = gm.turnCount.ToString();
        gm.phaseChangeButton.GetComponentInChildren<TMP_Text>().text = "COMBAT";
        gm.currentFocusCardLogic = null;
        gm.isNotLoading = true;
        gm.StateReset();
        CostPhase(player);
    }

    public void DrawPhase(PlayerManager player)
    {
        gm.turnPhaseText.text = "Reinforcement";
        gm.phaseChangeButton.SetActive(false);
        gm.SwitchControl(player);
        gm.PhaseChange(Phase.DrawPhase);
        gm.DrawCard(1, player);
        gm.currentFocusCardLogic = null;
        gm.StateReset();
        gm.AllEffectsRefresh(player);
        gm.AllAttacksRefresh(player);
        gm.ShieldRefresh(player);
        CostPhase(player);
    }

    public void EndPhase(PlayerManager player)
    {
        gm.turnPhaseText.text = "Retreat";
        gm.PhaseChange(Phase.EndPhase);
        gm.ChainResolution();
        gm.currentFocusCardLogic = null;
        gm.StateChange(GameState.TurnEnd);
        gm.StateReset();
        if (player == gm.BluePlayerManager)
            SwitchTurn(gm.RedPlayerManager);
        else
            SwitchTurn(gm.BluePlayerManager);

    }

    public void SwitchTurn(PlayerManager player)
    {
        gm.turnPlayer = player;
        gm.turnCount++;
        gm.turnCountText.text = gm.turnCount.ToString();
        gm.currentFocusCardLogic = null;
        gm.StateReset();
        DrawPhase(player);
    }

    public void CostPhase(PlayerManager player)
    {
        gm.turnPhaseText.text = "Recovery";
        gm.PhaseChange(Phase.CostPhase);
        int amount = player.costPhaseGain;
        gm.CostChange(amount, player, true);
        player.costPhaseGain++;
        gm.currentFocusCardLogic = null;
        gm.StateChange(GameState.Cost);
        gm.StateReset();
        MainPhase(player);
    }

    public void MainPhase(PlayerManager player)
    {
        gm.turnPhaseText.text = "Deployment";
        PhaseButtonCheck(player);
        gm.PhaseChange(Phase.MainPhase);
        gm.currentFocusCardLogic = null;
        gm.StateReset();
    }

    public void BattlePhase(PlayerManager player)
    {
        gm.turnPhaseText.text = "Combat";
        PhaseButtonCheck(player);
        gm.PhaseChange(Phase.BattlePhase);
        gm.currentFocusCardLogic = null;
        gm.StateReset();
    }

    public void TriggerPhaseChange()
    {
        if (gm.currentPhase == Phase.MainPhase)
        {
            gm.phaseChangeButton.GetComponentInChildren<TMP_Text>().text = "END TURN";
            BattlePhase(gm.turnPlayer);
        }
        else if (gm.currentPhase == Phase.BattlePhase)
        {
            gm.phaseChangeButton.GetComponentInChildren<TMP_Text>().text = "COMBAT";
            EndPhase(gm.turnPlayer);
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

