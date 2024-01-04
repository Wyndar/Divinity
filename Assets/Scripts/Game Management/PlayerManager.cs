using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public Game_Manager gm;
    public UIManager ui;
    public SaveManager SaveManager;
    public PlayerManager enemy;
    public AIManager AIManager;
    public UnderworldManager underworldManager;

    public string PlayerName, PlayerID, DeckPath, HeroDeckPath;

    public bool isAI, isLocal, wentFirst;

    public List<string> deckListID = new();
    public List<string> heroDeckListID = new();

    public List<Card> deckList = new();
    public List<Card> heroDeckList = new();

    public List<CardLogic> handLogicList = new();
    public List<CardLogic> fieldLogicList = new();
    public List<CardLogic> deckLogicList = new();
    public List<CardLogic> heroDeckLogicList = new();
    public List<CardLogic> graveLogicList = new();
    public List<CardLogic> playableLogicList = new();
    public List<CardLogic> canAttackLogicList = new();
    public List<CardLogic> canUseEffectLogicList = new();

    public List<int> canUseEffectNumber = new();
    public List<int> canUseEffectSubNumber = new();

    public GodLogic heroCardLogic;

    public Transform activationZone;

    public GameObject deck, grave, hero, shield, shieldPanel, activateShieldButton, ignoreShieldButton, raycastBlocker, graveTarget, deckTarget, heroEffectTarget, heroAttackTarget, deckSearchButton, graveSearchButton, hourglassIcon, heroHpStatus, heroAtkStatus, heroNumbers, heroStatus, heroStatus2;

    public GameObject[] cardSlots, handSlots, effectActivationButtons, attackDeclarationButtons, atkIcons, hpIcons, effectTargets, attackTargets, fieldIcons, statusIcons, statusIcons2, damageNums, attackProjectileIcons, effectProjectileIcons;

    public bool[] isEmptyCardSlot, isEmptyHandSlot;

    public TMP_Text deckCountText, graveCountText, heroHpText, heroAtkText, shieldText, costText;

    public int deckCount, graveCount, shieldCount, costCount;

    public int costPhaseGain = 1;
    public int handSize;

    public void SetStat(int orderNum, Status status, int statusChangeAmount)
    {

        GameObject stat = statusIcons[orderNum];
        if (stat.GetComponent<StatusIconMoveAndFadeAway>().inUse)
            stat = statusIcons2[orderNum];
        GameObject num = damageNums[orderNum];
        ui.StatUpdate(status, statusChangeAmount, stat, num);
    }

    public void SetHeroStatus(Status status, int statusChangeAmount)
    {
        GameObject stat = heroStatus;
        if (stat.GetComponent<StatusIconMoveAndFadeAway>().inUse)
            stat = heroStatus2;
        ui.HeroStatUpdate(status, statusChangeAmount, stat, this);
    }

    public void SetStatusIcon(int orderNum, CardStatus status) =>
        ui.AddStatIcon(fieldIcons[orderNum], status);
}
