using UnityEngine;

public class Grow : MonoBehaviour
{
    public float growSpeed;
    public float minSize;
    public float maxSize;
    public bool autoActivate;
    private bool shouldGrow;

    void OnEnable()
    {
        if (autoActivate)
        {
            transform.localScale = new(minSize, minSize, transform.localScale.z);
            GrowBig();
        }
    }
    void Update()
    {
        if (shouldGrow)
        {
            if (transform.localScale.x < maxSize)
                transform.localScale = new(transform.localScale.x + maxSize * growSpeed * Time.deltaTime, transform.localScale.y + maxSize * growSpeed * Time.deltaTime, transform.localScale.z);
            else
                shouldGrow = false;
        }
    }
    public void GrowBig()=> shouldGrow = true;
}
