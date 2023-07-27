using TMPro;
using static Game_Manager;
using static PlayerManager;

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
                currentLocation = Location.Field;
                locationOrderNumber = slotNumber;
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

        cardController.SetStatus(locationOrderNumber, status, value);
        OnFieldAtkRefresh();
        OnFieldHpRefresh();
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
        cardController.armorIcons[locationOrderNumber].SetActive(false);
        cardController.isEmptyCardSlot[locationOrderNumber] = true;
        cardController.fieldLogicList.Remove(this);
        cardController.SetStatus(locationOrderNumber, Status.Death, 0);
        playLogic.MoveToGrave();
        gm.StateChange(GameState.Death);
    }
}