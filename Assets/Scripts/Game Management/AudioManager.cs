using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    private GameObject BGMusicPlayer, BattleMusicPlayer;
    private AudioSource BattleMusicSource, BGAudioSource;

    [SerializeField]
    private List<AudioClip> BGAudioClips, DamageAudioClips, DeathAudioClips;

    public GameObject sfxAudioPrefab;

    public AudioClip select, unselect, click, draw, shuffleDeck, shuffleHand, playCard, flipCard, passTurn, error, battlePhase, attackDeclaration,
    attackResolution, attackResolutionArmored, effectActivation, effectResolution, summon, buff;

    private AudioClip battleMusic, hyperBattleMusic;

    private float BGMVolume = 1f;
    public void FindBattleOBJ()
    {
        BattleMusicPlayer = GameObject.Find("Battle Phase Music");
        BattleMusicSource = BattleMusicPlayer.GetComponent<AudioSource>();
        hyperBattleMusic = (AudioClip)Resources.Load("Music/Battle Phase Loop Music/Hyper War Drums", typeof(AudioClip));
        battleMusic = (AudioClip)Resources.Load("Music/Battle Phase Loop Music/Basic War Drums", typeof(AudioClip));
    }

    public void FindBGOBJ()
    {
        BGMusicPlayer = GameObject.Find("Background Music");
        BGAudioSource = BGMusicPlayer.GetComponent<AudioSource>();
        BGAudioClips.Clear();
        foreach (AudioClip audioClip in Resources.LoadAll($"Music/{SceneManager.GetActiveScene().name}", typeof(AudioClip)).Cast<AudioClip>())
            BGAudioClips.Add(audioClip);
    }

    public void BattlePhaseMusic(bool isHyper)
    {
        BattleMusicSource.clip = isHyper ? hyperBattleMusic : battleMusic;
        BattleMusicSource.Play();
        BGMVolume = BGAudioSource.volume;
        BGAudioSource.volume /= 5;
    }

    public void LoadSFX()
    {
        DamageAudioClips.Clear();
        DeathAudioClips.Clear();
        DamageAudioClips.AddRange(Resources.LoadAll("SFX/Generic Damage SFX", typeof(AudioClip)));
        DeathAudioClips.AddRange(Resources.LoadAll("SFX/Generic Death SFX", typeof(AudioClip)));
    }

    public void EndBattlePhaseMusic()
    {
        BattleMusicSource.Pause();
        BGAudioSource.volume = BGMVolume;
    }

    public void SelectRandomBGM()
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
