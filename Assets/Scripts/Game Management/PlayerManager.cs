using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PlayerManager : MonoBehaviour
{
	public Game_Manager gm;
    public SaveManager SaveManager;
    public PlayerManager enemy;

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

    public GodLogic heroCardLogic;

    public Transform activationZone;

    public GameObject deck, grave, hero, shield, shieldPanel, activateShieldButton, ignoreShieldButton, raycastBlocker;

    public GameObject[] cardSlots, handSlots, effectActivationButtons, attackDeclarationButtons, atkIcons, hpIcons;

    public bool[] isEmptyCardSlot, isEmptyHandSlot;

    public TMP_Text deckCountText, graveCountText, heroHpText, heroAtkText, shieldText, costText;

    public int deckCount, graveCount, shieldCount, costCount;

    public int costPhaseGain = 1;
    public int handSize;
}
