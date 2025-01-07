using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject trail, errorPanel, loadingScreenPrefab, canvas;
    [SerializeField] private Slider BGMSlider, SFXSlider;
    private bool ShowError = true, ShowTrail = true;
    private InputManager InputManager;
    private AudioManager AudioManager;
    private Coroutine trailCoroutine;
    private void OnEnable()
    {
        InputManager = GetComponent<InputManager>();
        AudioManager = GetComponent<AudioManager>();
        AudioManager.FindBGOBJ();
        AudioManager.SelectRandomBGM(null);
        AudioManager.LoadVolumeSettings();
        LoadVFXSettings();
        BGMSlider.value = PlayerPrefs.GetFloat("BGM Volume", 0.5f);
        SFXSlider.value = PlayerPrefs.GetFloat("SFX Volume", 0.5f);
        trail.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("SFX Volume", 0.5f);
        InputManager.OnStartTouch += TouchStart;
        InputManager.OnEndTouch += TouchEnd;
    }

    private void OnDisable()
    {
        InputManager.OnEndTouch -= TouchStart;
        InputManager.OnStartTouch -= TouchEnd;
    }
    private void LoadVFXSettings()
    {
        ShowError = PlayerPrefs.GetInt("Show Error", 1) == 1;
        ShowTrail = PlayerPrefs.GetInt("Show Move", 1) == 1;
    }

    public void ToggleError(bool showError)
    {
        ShowError = showError;
        PlayerPrefs.SetInt("Show Error", ShowError ? 1 : 0);
    }
    public void ToggleTrail(bool showTrail)
    {
        ShowTrail = showTrail;
        PlayerPrefs.SetInt("Show Move", ShowTrail ? 1 : 0);
    }
    public void EnablePanel(GameObject panel) => panel.SetActive(true);
    public void DisablePanel(GameObject panel) => panel.SetActive(false);
    public void ExitApp() => Application.Quit();
    private void TouchStart(Vector2 pos, float time)
    {
        AudioManager.NewAudioPrefab(AudioManager.click);
        trail.SetActive(ShowTrail);
        if (ShowTrail)
            trailCoroutine = StartCoroutine(Trail());
    }
    private void TouchEnd(Vector2 pos, float time)
    {
        if (trailCoroutine != null)
            StopCoroutine(trailCoroutine);
        trail.SetActive(false);
    }

    public Vector3 ScreenToWorld(Vector3 position)
    {
        position.z = 25f;
        return Camera.main.ScreenToWorldPoint(position);
    }
    public void ErrorCodePanel(string errorText)
    {
        if (ShowError)
            return;
        GameObject ep = Instantiate(errorPanel, errorPanel.transform.parent.transform);
        AudioManager.NewAudioPrefab(AudioManager.error);
        ep.SetActive(true);
        ep.GetComponentInChildren<TMP_Text>().text = errorText;
    }

    private IEnumerator Trail()
    {
        while (trail.activeInHierarchy)
        {
            trail.transform.position = ScreenToWorld(InputManager.CurrentFingerPosition);
            yield return null;
        }
    }
    public void LoadScene(int sceneNum)
    {
        SceneHandler sceneHandler = Instantiate(loadingScreenPrefab, canvas.transform).GetComponent<SceneHandler>();
        sceneHandler.LoadScene(sceneNum);
    }
}
