using UnityEngine;

public class UIManager : MonoBehaviour
{
	public Game_Manager gm;

    public void UIUpdate(PlayerManager playerManager)
    {
        playerManager.deckCount = playerManager.deckLogicList.Count;
        playerManager.graveCount = playerManager.graveLogicList.Count;
        playerManager.shieldCount = playerManager.heroDeckLogicList.Count;

        playerManager.deckCountText.text = playerManager.deckCount.ToString();
        playerManager.graveCountText.text = playerManager.graveCount.ToString();
        playerManager.shieldText.text = playerManager.shieldCount.ToString();
        playerManager.costText.text = playerManager.costCount.ToString();
        playerManager.heroAtkText.text = playerManager.heroCardLogic.gameObject.GetComponent<CombatantLogic>().currentAtk.ToString();
        playerManager.heroHpText.text = playerManager.heroCardLogic.gameObject.GetComponent<CombatantLogic>().currentHp.ToString();
    }
}

