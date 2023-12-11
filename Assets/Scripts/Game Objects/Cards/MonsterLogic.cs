using TMPro;

public class MonsterLogic : CardLogic
{
    public Game_Manager gm;
    public CombatantLogic combatLogic;
    public PlayableLogic playLogic;

    public void OnFieldAtkRefresh() => cardController.atkIcons[locationOrderNumber].GetComponentInChildren<TMP_Text>().text = combatLogic.currentAtk.ToString();
    public void OnFieldHpRefresh() => cardController.hpIcons[locationOrderNumber].GetComponentInChildren<TMP_Text>().text = combatLogic.currentHp.ToString();

    public void MonsterSummon(PlayerManager player)
    {
        for (int slotNumber = 0; slotNumber < player.isEmptyCardSlot.Length; slotNumber++)
        {
            if (player.isEmptyCardSlot[slotNumber] == true)
            {
                transform.position = player.cardSlots[slotNumber].transform.position;
                player.isEmptyCardSlot[slotNumber] = false;
                if (gm.isActivatingEffect)
                    LocationChange(gm.currentFocusCardLogic.focusEffect, gm.currentFocusCardLogic.focusEffect.effectsUsed[gm.currentFocusCardLogic.subCountNumber], Location.Field, slotNumber);
                else
                    LocationChange(null, EffectsUsed.Undefined, Location.Field, slotNumber);

                player.fieldLogicList.Add(this);
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
        combatLogic.attacksLeft = combatLogic.maxAttacks;
        gm.StateChange(GameState.Summon);
    }

    override public void StatAdjustment(int value, Status status)
    {
        cardController.SetStat(locationOrderNumber, status, value);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
        DeathCheck();
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
        combatLogic.cardStatuses.Clear();
        combatLogic.hasDoneCountdown = false;
        cardController.atkIcons[locationOrderNumber].SetActive(false);
        cardController.hpIcons[locationOrderNumber].SetActive(false);
        cardController.armorIcons[locationOrderNumber].SetActive(false);
        cardController.isEmptyCardSlot[locationOrderNumber] = true;
        cardController.fieldLogicList.Remove(this);
        cardController.SetStat(locationOrderNumber, Status.Death, 0);
        playLogic.MoveToGrave();
        gm.StateChange(GameState.Death);
    }
}