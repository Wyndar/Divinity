using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameBattleManager gm;
    public SecondaryUIManager ui;
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

    [NonSerialized]
    public List<CardLogic> handLogicList = new();
    public List<CardLogic> fieldLogicList = new();
    public List<CardLogic> deckLogicList = new();
    public List<CardLogic> heroDeckLogicList = new();
    public List<CardLogic> graveLogicList = new();
    public List<CardLogic> playableLogicList = new();
    public List<CardLogic> canAttackLogicList = new();
    public List<CardLogic> canUseEffectLogicList = new();

    public List<SubEffect> canUseSubEffectList = new();

    public GodLogic heroCardLogic;

    public Transform activationZone;

    public GameObject deck, grave, hero, shield, shieldPanel, activateShieldButton, ignoreShieldButton, raycastBlocker, graveTarget,
        deckTarget, heroEffectTarget, heroAttackTarget, deckSearchButton, graveSearchButton, hourglassIcon, heroHpStatus,
        heroAtkStatus, heroNumbers, heroStatus, heroStatus2, hand, effectActivationButton;

    public List<HandSlot> handSlots;
    public List<CardSlot> cardSlots;

    public TMP_Text deckCountText, graveCountText, heroHpText, heroAtkText;

    public int deckCount, graveCount, shieldCount, costCount;
    public Blood[] bloods;
    public Color[] bloodColours, shieldColours;
    public SpriteRenderer[] shieldSprites;
    public Color playerColor;

    public int costPhaseGain = 1;
    public int handSize;

    public void BloodGain(Attunement attunement, int count)
    {
        int tempCount = count;
        foreach (Blood b in bloods)
        {
            if (b.bloodState is not BloodState.Inactive and not BloodState.Undefined)
                continue;
            b.BloodGain();
            tempCount--;
            costCount++;
            gm.StateChange(GameState.Cost);
            if (tempCount == 0)
                break;
        }
        if (attunement != Attunement.Untuned)
            BloodAttunement(attunement, count);
    }
    public void BloodLoss(List<Attunement> attunements, int count)
    {
        foreach (Attunement attunement in attunements)
        {
            foreach (Blood b in bloods)
            {
                if (b.bloodState != BloodState.Active || b.attunement != attunement)
                    continue;
                b.BloodLoss();
                count--;
                costCount--;
                gm.StateChange(GameState.Cost);
                if (count == 0)
                    return;
            }
        }
    }

    public int BloodAttunementCheck(Attunement attunement)
    {
        int count = 0;
        foreach(Blood b in bloods)
        {
            if (attunement != Attunement.Undefined || b.attunement == Attunement.Untuned)
                if (b.attunement != attunement || b.bloodState == BloodState.Inactive)
                    continue;

            count++;
        }
        return count;
    }
    public void BloodAttunement(Attunement attunement, int count)
    {
        foreach (Blood blood in bloods)
        {
            if (blood.attunement != Attunement.Untuned || blood.bloodState != BloodState.Active)
                continue;
            Attunement tempAttunement = Attunement.Undefined;
            if (attunement == Attunement.Undefined)
            {
                if (heroCardLogic.attunementRates.Count != 1)
                {
                    int attunementOdds = UnityEngine.Random.Range(1, 101);
                    int rateOdds = 0;
                    for (int i = 0; i < heroCardLogic.attunementRates.Count; i++)
                    {
                        rateOdds += heroCardLogic.attunementRates[i];
                        if (attunementOdds > rateOdds)
                            continue;
                        tempAttunement = heroCardLogic.dataLogic.attunements[i];
                        break;
                    }
                }
                else
                    tempAttunement = heroCardLogic.dataLogic.attunements[0];
            }
            else
                tempAttunement = attunement;
            blood.Attune(tempAttunement, bloodColours[(int)(tempAttunement - 1)]);
            count--;
            if (count == 0)
                break;
        }
    }

    public void SetShield(int shouldRemove, int count)
    {
        foreach (SpriteRenderer sprite in shieldSprites)
        {
            if (sprite.color != shieldColours[shouldRemove])
                continue;
            sprite.color = shouldRemove == 0 ? shieldColours[1] : shieldColours[0];
            count--;
            if (count == 0)
                break;
        }
    }    

    public void SetHeroStatus(Status status, int statusChangeAmount)
    {
        GameObject stat = heroStatus;
        if (stat.GetComponent<StatusIconMoveAndFadeAway>().inUse)
            stat = heroStatus2;
        ui.HeroStatUpdate(status, statusChangeAmount, stat, this);
    }
}
