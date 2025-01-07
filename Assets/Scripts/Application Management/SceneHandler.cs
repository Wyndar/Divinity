using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TMP_Text loadingPercentText,loadingElipsesText;

    private float loadPercentage;
    private Color startColor = Color.clear, endColor = Color.grey;

    public void LoadScene(int sceneNum) => StartCoroutine(LoadSceneCoroutine(sceneNum));
    private IEnumerator LoadSceneCoroutine(int sceneNum)
    {
        gameObject.GetComponent<Image>().color = startColor;
        loadingBar.value = 0;
        loadPercentage = 0;
        float timeSinceLastShift = 0;
        AsyncOperation loadAsync = SceneManager.LoadSceneAsync(sceneNum);
        loadAsync.allowSceneActivation = false;
        while (!loadAsync.isDone)
        {
            timeSinceLastShift += Time.deltaTime;
            loadPercentage = Mathf.MoveTowards(loadPercentage, loadAsync.progress, Time.deltaTime);
            loadingBar.value = loadPercentage;
            loadingPercentText.text = $"{Mathf.FloorToInt(loadPercentage * 100)}%";
            gameObject.GetComponent<Image>().color = Color.Lerp(startColor, endColor, loadPercentage);
            if (timeSinceLastShift > 1)
            {
                if (loadingElipsesText.text.Length < 3)
                    loadingElipsesText.text += ".";
                else
                    loadingElipsesText.text = ".";
                timeSinceLastShift = 0;
            }
            if (loadPercentage >= 0.9f)
            {
                loadPercentage = 1;
                loadingPercentText.text = "99%";
                gameObject.GetComponent<Image>().color = endColor;
                loadingBar.value = 1;
                loadAsync.allowSceneActivation = true;
            }
            yield return null;
        }
        yield break;
    }
}