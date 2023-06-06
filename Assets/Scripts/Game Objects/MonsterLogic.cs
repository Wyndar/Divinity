using TMPro;

public class MonsterLogic : CardLogic
{
    public Game_Manager gm;
    public CombatantLogic combatLogic;
    public PlayableLogic playLogic;

    public void OnFieldAtkRefresh() => cardOwner.atkIcons[locationOrderNumber].GetComponentInChildren<TMP_Text>().text = GetComponent<CombatantLogic>().currentAtk.ToString();
    public void OnFieldHpRefresh() => cardOwner.hpIcons[locationOrderNumber].GetComponentInChildren<TMP_Text>().text = GetComponent<CombatantLogic>().currentHp.ToString();

    public void MonsterSummon(PlayerManager player)
    {
        for (int slotNumber = 0; slotNumber < player.isEmptyCardSlot.Length; slotNumber++)
        {
            if (player.isEmptyCardSlot[slotNumber] == true)
            {
                transform.position = player.cardSlots[slotNumber].transform.position;
                player.isEmptyCardSlot[slotNumber] = false;
                currentLocation = Location.Field;
                locationOrderNumber = slotNumber;
                combatLogic.currentAtk = combatLogic.atk;
                combatLogic.maxHp = combatLogic.hp;
                combatLogic.currentHp = combatLogic.hp;
                player.atkIcons[locationOrderNumber].SetActive(true);
                player.hpIcons[locationOrderNumber].SetActive(true);
                OnFieldAtkRefresh();
                OnFieldHpRefresh();
                gm.ShuffleHand(cardOwner);
                break;
            }
        }
        gameObject.GetComponent<CombatantLogic>().attacksLeft = gameObject.GetComponent<CombatantLogic>().maxAttacks;
        gm.StateChange(Game_Manager.GameState.Summon);
    }

    public void DeathCheck()
    {
        if (combatLogic.currentHp > 0)
            return;
        MonsterDeath();
    }

    public void MonsterDeath()
    {
        combatLogic.currentHp = 0;
        cardOwner.atkIcons[locationOrderNumber].SetActive(false);
        cardOwner.hpIcons[locationOrderNumber].SetActive(false);
        cardOwner.isEmptyCardSlot[locationOrderNumber] = true;
        playLogic.MoveToGrave();
        gm.StateChange(Game_Manager.GameState.Death);
    }
}


