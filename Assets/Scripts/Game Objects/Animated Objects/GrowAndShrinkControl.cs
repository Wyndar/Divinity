using UnityEngine;
using TMPro;
[RequireComponent(typeof(Grow))]
[RequireComponent (typeof(ShrinkAndFadeAway))]
public class GrowAndShrinkControl : MonoBehaviour
{
    public float timeToDisappear;
    public float timeSinceAppearance;
    public TMP_Text text;
    // Start is called before the first frame update

    void OnEnable() => timeSinceAppearance = 0;

    // Update is called once per frame
    void Update()
    {
        timeSinceAppearance += Time.deltaTime;
        if (timeSinceAppearance >= timeToDisappear && timeToDisappear != 0f)
            GetComponent<ShrinkAndFadeAway>().FadeAway();
    }
}
