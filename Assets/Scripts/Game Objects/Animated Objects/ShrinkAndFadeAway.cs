using UnityEngine;

public class ShrinkAndFadeAway : MonoBehaviour
{
    public float shrinkSpeed;
    public float minSize;
    public float maxSize;
    public bool destroyGO;
    public bool autoActivate;
    private bool shouldShrink;

    void OnEnable()
    {
        if (autoActivate)
            FadeAway();
    }

    void Update()
    {
        if (shouldShrink)
        {
            if (transform.localScale.x > minSize)
                transform.localScale = new(transform.localScale.x - maxSize * shrinkSpeed * Time.deltaTime, transform.localScale.y - maxSize * shrinkSpeed * Time.deltaTime, transform.localScale.z);
            if (transform.localScale.x <= minSize)
            {
                shouldShrink = false;
                transform.localScale=new(maxSize,maxSize,transform.localScale.z);
                if (destroyGO)
                    Destroy(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }
    }

    public void FadeAway()=>shouldShrink=true;
}
