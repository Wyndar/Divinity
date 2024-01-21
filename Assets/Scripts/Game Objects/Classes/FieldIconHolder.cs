using UnityEngine;
using TMPro;
public class FieldIconHolder : MonoBehaviour
{
    public TMP_Text amountText;
    public TMP_Text durationText;
    public CardStatus cardStatus;
    public void DestroyThis()=>Destroy(gameObject);
}
