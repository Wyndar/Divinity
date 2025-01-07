using TMPro;
using UnityEngine;

public class LoadingHandler : MonoBehaviour
{
    public GameObject loadImage;
    public TMP_Text loadingElipsesText;
    private float timeSinceLastShift;
    private void Update()
    {
        timeSinceLastShift += Time.deltaTime;
        if (timeSinceLastShift > 1)
        {
            if (loadingElipsesText.text.Length < 3)
                loadingElipsesText.text += ".";
            else
                loadingElipsesText.text = ".";
            timeSinceLastShift = 0;
        }
        loadImage.transform.Rotate(0, 0, 1, Space.Self);
    }
}
