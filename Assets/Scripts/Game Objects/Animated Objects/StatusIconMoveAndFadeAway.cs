using UnityEngine;

public class StatusIconMoveAndFadeAway : MonoBehaviour
{
    public float lifeTime = 0.3f;
    private float time;
    private Vector3 originalPosition;

    private void OnEnable()
    {
        originalPosition = transform.position;
        time = lifeTime;
    }

    private void Update()
    {
        time -= Time.deltaTime;
        if (time > 0)
            transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        if (time <= 0)
        {
            transform.position = originalPosition;
            gameObject.SetActive(false);
        }
    }
}