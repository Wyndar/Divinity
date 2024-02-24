using UnityEngine;
[RequireComponent (typeof(ShrinkAndFadeAway))]
public class GrowAndShrinkControl : MonoBehaviour
{
    public bool shouldMove;
    public float timeToDisappear;
    public float timeSinceAppearance;
    private Vector3 originalPosition;

    private void OnEnable()
    {
        originalPosition = transform.position;
        timeSinceAppearance = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceAppearance += Time.deltaTime;
        if (timeSinceAppearance < timeToDisappear && shouldMove)
            transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        if (timeSinceAppearance >= timeToDisappear && timeToDisappear != 0f)
        {
            transform.position = originalPosition;
            GetComponent<ShrinkAndFadeAway>().FadeAway();
        }
    }
}
