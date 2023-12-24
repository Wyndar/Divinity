using UnityEngine;

[RequireComponent (typeof(AudioSource))]
public class DestroyAfterPlayingAudio : MonoBehaviour
{
    public bool hasPlayed;
    void Update()
    {
        if (hasPlayed)
            if (!GetComponent<AudioSource>().isPlaying)
                Destroy(gameObject);
    }
}
