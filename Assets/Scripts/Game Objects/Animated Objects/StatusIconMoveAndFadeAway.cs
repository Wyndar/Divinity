using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusIconMoveAndFadeAway : MonoBehaviour
{
    public bool inUse;
    private float lifeTime = 1f;
    private float time;
    private Vector3 originalPosition;

    private void OnEnable()
    {
        originalPosition = transform.position;
        time = lifeTime;
        inUse = true;
    }

    private void Update()
    {
        time -= Time.deltaTime;
        if (time > 0)
            transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        if (time <= 0)
        {
            transform.position = originalPosition;
            GetComponent<Image>().color = Color.white;
            if (transform.GetChild(0).gameObject.activeInHierarchy)
            {
                transform.GetComponentInChildren<TMP_Text>().color = Color.black;
                transform.GetChild(0).gameObject.SetActive(false);
            }
            inUse = false;
            gameObject.SetActive(false);
        }
    }
}