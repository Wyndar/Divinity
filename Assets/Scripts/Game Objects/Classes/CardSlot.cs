using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private GameObject effectActivationButtons, attackDeclarationButtons, moveButtons, atkIcons, hpIcons, effectTargets,
        attackTargets, fieldIcons, statusIcons, statusIcons2, damageNums, attackProjectileIcons, effectProjectileIcons;
    public bool isFrontline;
    public int column;
    public int row;
    public CardLogic cardInZone;
    public PlayerManager controller;
    public SpriteRenderer sprite;
    public GameObject effectActivationButton, attackDeclarationButton, moveButton, atkIcon, hpIcon, effectTarget,
        attackTarget, fieldIcon, statusIcon, statusIcon2, damageNum, attackProjectileIcon, effectProjectileIcon;
    private GameBattleManager gm;

    public void InitializeSlot(GameBattleManager game_Manager)
    {
        sprite = GetComponent<SpriteRenderer>();
        gm = game_Manager;
        effectActivationButton = effectActivationButtons.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        attackDeclarationButton = attackDeclarationButtons.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        moveButton = moveButtons.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        atkIcon = atkIcons.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        hpIcon = hpIcons.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        effectTarget = effectTargets.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        attackTarget = attackTargets.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        fieldIcon = fieldIcons.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        statusIcon = statusIcons.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        statusIcon2 = statusIcons2.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
        damageNum =damageNums.transform.GetChild(row - 1).GetChild(column - 1).gameObject;
    }
    public void ChangeController(PlayerManager player)
    {
        controller = player;
        sprite.color = player.playerColor;
    }
    public void SetStat(Status status, int statusChangeAmount)
    {
        GameObject stat = statusIcon;
        if (stat.GetComponent<StatusIconMoveAndFadeAway>().inUse)
            stat = statusIcon2;
        GameObject num = damageNum;
        controller.ui.StatUpdate(status, statusChangeAmount, stat, num);
    }
    public void SetStatusIcon(CardStatus status) =>
        controller.ui.AddStatIcon(fieldIcon, status);
    public void SelectSlot()
    {
        if (gm.currentFocusCardSlot != null)
            gm.currentFocusCardSlot.DeselectSlot();
        if (gm.currentFocusCardLogic != null)
        {
            if (gm.currentFocusCardLogic.dataLogic.type != Type.Fighter 
                || gm.currentFocusCardLogic.dataLogic.cardController != gm.BluePlayerManager)
                return;
            sprite.color = Color.green;
            if (isFrontline || cardInZone != null)
                sprite.color = Color.grey;
            if (gm.currentFocusCardLogic.dataLogic.cardController != controller)
                sprite.color = Color.red;
        }
        else
            sprite.color = Color.yellow;
        gm.currentFocusCardSlot = this;
    }
    public void DeselectSlot()
    {
        sprite.color = controller.playerColor;
        gm.currentFocusCardSlot = null;
    }
    public void CleanUpIcons()
    {
        atkIcon.SetActive(false);
        hpIcon.SetActive(false);
        List<GameObject> allChildren = new();
        foreach (Transform child in fieldIcon.transform)
            allChildren.Add(child.gameObject);
        foreach (GameObject child in allChildren)
            Destroy(child);
    }    
}
