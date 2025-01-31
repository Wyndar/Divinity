using TMPro;
using UnityEngine;

public class CardLogic : MonoBehaviour
{
    public GameBattleManager gameManager;
    public AudioManager audioManager;
    public SecondaryUIManager U_I;

    public CardVisualsLogic visualsLogic;
    public CardEffectLogic effectLogic;
    public CardTargetingLogic targetingLogic;
    public CardTriggerLogic triggerLogic;
    public CardDataLogic dataLogic;

    public void EffectActivation(SubEffect subEffect) => effectLogic.EffectActivation(subEffect);
    public void EffectResolution(SubEffect subEffect) => effectLogic.EffectResolution(subEffect);
    public void FinishResolution(SubEffect subEffect) => effectLogic.FinishResolution(subEffect);
    public void GetPhaseTriggers(Phase phase) => triggerLogic.GetPhaseTriggers(phase);
    public void GetEffectTriggers(SubEffect subEffect, CardLogic triggerCard) => triggerLogic.GetEffectTriggers(subEffect, triggerCard);
    public void GetStateTriggers(GameState gameState, CardLogic cardLogic) => triggerLogic.GetStateTriggers(gameState, cardLogic);
    public void EffectHandler(SubEffect subEffect) => effectLogic.EffectHandler(subEffect);
    public virtual void StatAdjustment(int value, Status status) => throw new MissingReferenceException($"Failed virtual override for status of {dataLogic.cardName}");
    public void LocationChange(Location location, int num) => dataLogic.LocationChange(location, num);
    public void SetFocusCardLogic()
    {
        if (gameManager.currentFocusCardLogic != null)
            gameManager.currentFocusCardLogic.RemoveFocusCardLogic();
        gameManager.currentFocusCardLogic = this;
        if (dataLogic.type != Type.God)
            visualsLogic.ToggleCardOutline(true);
    }

    public void RemoveFocusCardLogic()
    {
        gameManager.currentFocusCardLogic = null;
        if (dataLogic.type != Type.God)
            visualsLogic.ToggleCardOutline(false);
    }
}