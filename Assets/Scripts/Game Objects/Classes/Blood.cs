using UnityEngine;

public class Blood : MonoBehaviour
{
    public Attunement attunement = Attunement.Undefined;
    public BloodState bloodState = BloodState.Undefined;

    public void Attune(Attunement toSet, Color color)
    {
        attunement = toSet;
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = color;
    }

    public void BloodGain()
    {
        attunement = Attunement.Untuned;
        bloodState = BloodState.Active;
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
    }
    public void BloodLoss()
    {
        attunement = Attunement.Untuned;
        bloodState = BloodState.Inactive;
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.black;
    }
}
