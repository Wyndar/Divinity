using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    private GameObject BGMusicPlayer;
    private AudioSource BGAudioSource;

    [SerializeField]
    private List<AudioClip> BGAudioClips;

    public GameObject sfxAudioPrefab;

    public AudioClip select;
    public AudioClip unselect;
    public AudioClip click;
    public AudioClip draw;
    public AudioClip shuffleDeck;
    public AudioClip shuffleHand;
    public AudioClip playCard;
    public AudioClip flipCard;
    public AudioClip passTurn;
    public AudioClip error;

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

    public AudioSource NewAudioPrefab(AudioClip audioClip)
    {
        GameObject newAudioPrefab = Instantiate(sfxAudioPrefab);
        AudioSource newAudio = newAudioPrefab.GetComponent<AudioSource>();
        newAudio.clip = audioClip;
        newAudio.Play();
        newAudio.GetComponent<DestroyAfterPlayingAudio>().hasPlayed = true;
        return newAudio;
    }
}
