using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    private GameObject BGMusicPlayer;
    private GameObject BattleMusicPlayer;
    private AudioSource BattleMusicSource;
    private AudioSource BGAudioSource;

    [SerializeField]
    private List<AudioClip> BGAudioClips;
    [SerializeField]
    private List<AudioClip> DamageAudioClips;
    [SerializeField]    
    private List<AudioClip> DeathAudioClips;

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
    public AudioClip battlePhase;
    public AudioClip attackDeclaration;
    public AudioClip attackResolution;
    public AudioClip attackResolutionArmored;
    public AudioClip effectActivation;
    public AudioClip effectResolution;
    public AudioClip summon;
    public AudioClip buff;


    public void FindBGOBJ()
    {
        BGMusicPlayer = GameObject.Find("Background Music");
        BGAudioSource = BGMusicPlayer.GetComponent<AudioSource>();
        BGAudioClips.Clear();
        foreach (AudioClip audioClip in Resources.LoadAll($"Music/{SceneManager.GetActiveScene().name}", typeof(AudioClip)))
            BGAudioClips.Add(audioClip);
    }

    public void BattlePhaseMusic(bool isHyper)
    {
        BattleMusicPlayer = GameObject.Find("Battle Phase Music");
        BattleMusicSource = BattleMusicPlayer.GetComponent<AudioSource>();
        BattleMusicSource.clip = isHyper ? (AudioClip)Resources.Load("Music/Battle Phase Loop Music/Hyper War Drums", typeof(AudioClip)) : (AudioClip)Resources.Load("Music/Battle Phase Loop Music/Basic War Drums", typeof(AudioClip));
        BattleMusicSource.Play();
        BGAudioSource.volume = 0.2f;
    }

    public void LoadSFX()
    {
        DamageAudioClips.Clear();
        DeathAudioClips.Clear();
        foreach (AudioClip audioClip in Resources.LoadAll("SFX/Generic Damage SFX", typeof(AudioClip)))
            DamageAudioClips.Add(audioClip);
        foreach (AudioClip audioClip in Resources.LoadAll("SFX/Generic Death SFX", typeof(AudioClip)))
            DeathAudioClips.Add(audioClip);
    }

    public void EndBattlePhaseMusic()
    {
        BattleMusicSource.Pause();
        BGAudioSource.volume = 0.5f;
    }

    public void SelectRandomBGMusic()
    {
        if (BGAudioClips.Count == 0)
            return;
        int ranNum = Random.Range(0, BGAudioClips.Count);
        BGAudioSource.clip= BGAudioClips[ranNum];
        BGAudioSource.Play();
    }

    public AudioSource SelectRandomDamageSFX()
    {
        if (DamageAudioClips.Count == 0)
            return null;
        int ranNum = Random.Range(0, DamageAudioClips.Count);
        return NewAudioPrefab(DamageAudioClips[ranNum]);
    }
    public AudioSource SelectRandomDeathSFX()
    {
        if (DeathAudioClips.Count == 0)
            return null;
        int ranNum = Random.Range(0, DeathAudioClips.Count);
        return NewAudioPrefab(DeathAudioClips[ranNum]);
    }

    public AudioSource SelectCharacterDamageSFX(string cardID)
    {
        AudioClip clip = (AudioClip)Resources.Load($"SFX/Character Damage SFX/{cardID}", typeof(AudioClip));
        if (clip == null)
            return SelectRandomDamageSFX();
        return NewAudioPrefab(clip);
    }

    public AudioSource SelectCharacterDeathSFX(string cardID)
    {
        AudioClip clip = (AudioClip)Resources.Load($"SFX/Character Death SFX/{cardID}", typeof(AudioClip));
        if (clip == null)
            return SelectRandomDeathSFX();
        return NewAudioPrefab(clip);
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
