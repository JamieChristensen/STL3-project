using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] clips;


    public AudioSource audioSource; //Public for access to audio-clip length - for player death effect.


    public void PlaySound()
    {
        if (clips.Length == 0)
        {
            Debug.Log("No sound clip in SoundPlayer");
            return;
        }
        audioSource.clip = clips[Random.Range(0, clips.Length)];
        audioSource.Play();
    }

    public void PlaySound(float timeInClip, int clip)
    {
        if (clips.Length == 0)
        {
            Debug.Log("No sound clip in SoundPlayer");
            return;
        }
        audioSource.clip = clips[clip];
        if (audioSource.clip.length < timeInClip)
        {   
            Debug.Log("Time in clip exceeded clip length in SoundPlayer");
            return;
        }
        audioSource.time = timeInClip;
        audioSource.Play();
    }
}
