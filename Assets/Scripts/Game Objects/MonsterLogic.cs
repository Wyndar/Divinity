﻿using TMPro;

public class MonsterLogic : CardLogic
{
    public Game_Manager gm;
    public CombatantLogic combatLogic;
    public PlayableLogic playLogic;

    public void OnFieldAtkRefresh() => cardController.atkIcons[locationOrderNumber].GetComponentInChildren<TMP_Text>().text = GetComponent<CombatantLogic>().currentAtk.ToString();
    public void OnFieldHpRefresh() => cardController.hpIcons[locationOrderNumber].GetComponentInChildren<TMP_Text>().text = GetComponent<CombatantLogic>().currentHp.ToString();

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
        cardController.atkIcons[locationOrderNumber].SetActive(false);
        cardController.hpIcons[locationOrderNumber].SetActive(false);
        cardController.isEmptyCardSlot[locationOrderNumber] = true;
        playLogic.MoveToGrave();
        gm.StateChange(Game_Manager.GameState.Death);
    }
}


