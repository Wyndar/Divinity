using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollCardImage : MonoBehaviour, IPointerClickHandler
{
	public CardLogic cardLogic;
	public Game_Manager Game_Manager;
    public UXManager UXManager;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        CardLogic focusCard = Game_Manager.currentFocusCardLogic;
        cardLogic.ManualTargetAcquisition(focusCard.effectCountNumber, focusCard.subCountNumber);
        UXManager.DisableCardScrollScreen();
    }
}

