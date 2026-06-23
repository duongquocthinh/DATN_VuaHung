using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip grindingSound;
    [SerializeField] private AudioClip cookingSound;
    [SerializeField] private AudioClip successSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }
    }

    public void PlayPickup()
    {
        PlayOneShot(pickupSound);
    }

    public void PlayGrinding()
    {
        PlayOneShot(grindingSound);
    }

    public void PlayCooking()
    {
        PlayOneShot(cookingSound);
    }

    public void PlaySuccess()
    {
        PlayOneShot(successSound);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
