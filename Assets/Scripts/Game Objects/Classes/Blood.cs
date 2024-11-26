using UnityEngine;

public class Blood : MonoBehaviour
{
    public Attunement attunement = Attunement.Undefined;
    public BloodState bloodState = BloodState.Undefined;

    public void Attune(Attunement toSet, Color color)
    {
        attunement = toSet;
        GetComponentInChildren<SpriteRenderer>().color = color;
    }

    public void BloodGain()
    {
        attunement = Attunement.Untuned;
        bloodState = BloodState.Active;
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
    }
    public void BloodLoss()
    {
        attunement = Attunement.Untuned;
        bloodState = BloodState.Inactive;
        GetComponentInChildren<SpriteRenderer>().color = Color.black;
    }
}
