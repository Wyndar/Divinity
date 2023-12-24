using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    public GameObject BGMusicPlayer;
    public AudioSource BGAudioSource;
    public List<AudioClip> BGAudioClips;

    public void OnEnable()
    {
        BGMusicPlayer = GameObject.Find("Background Music");
        BGAudioSource=BGMusicPlayer.GetComponent<AudioSource>();
        BGAudioClips.Clear();
        foreach (AudioClip audioClip in Resources.LoadAll($"Music/{SceneManager.GetActiveScene().name}", typeof(AudioClip)))
            BGAudioClips.Add(audioClip);
    }

    public void SelectRandomBGMusic()
    {
        if (BGAudioClips.Count == 0)
            return;
        int ranNum = Random.Range(0, BGAudioClips.Count);
        BGAudioSource.clip= BGAudioClips[ranNum];
        BGAudioSource.Play();
    }
}
