using TMPro;
using UnityEngine;

public class CardVisualsLogic : MonoBehaviour
{
    public Transform cardBack, cardImage, cardImageBorder, cardOutline, textCanvas;
    public Sprite image;
    public bool isFaceDown, isNormalColour;
    public Transform EnergyIcon;
    public TMP_Text costText, ATKText, HPText;
    private CardLogic cardLogic;
    public void Initialize() => cardLogic = GetComponent<CardLogic>();
    public void Flip(bool facedown)
    {
        isFaceDown = facedown;
        cardBack.gameObject.SetActive(facedown);
        cardImage.gameObject.SetActive(!facedown);
        cardImageBorder.gameObject.SetActive(!facedown);
        textCanvas.gameObject.SetActive(!facedown);
    }

    public void ToggleCardOutline(bool on)
    {
        if (cardLogic.dataLogic.currentLocation != Location.Grave && cardLogic.dataLogic.type != Type.God)
            cardOutline.gameObject.SetActive(on);
        else
            cardLogic.dataLogic.cardOwner.underworldManager.outline.SetActive(on);
    }

    public void GreyScaleEffect(bool activate)
    {
        isNormalColour = !activate;
        cardImage.GetComponent<SpriteRenderer>().color = activate ? Color.grey : Color.white;
        cardImageBorder.GetComponent<SpriteRenderer>().color = activate ? Color.grey : Color.white;
    }
}
