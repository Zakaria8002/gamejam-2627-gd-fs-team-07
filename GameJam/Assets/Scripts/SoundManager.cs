using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioClip gunClip;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void GunSound()
    {
        if (audioSource == null) return;

        if (gunClip != null)
        {
            audioSource.PlayOneShot(gunClip);
        }
        else if (audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
}
