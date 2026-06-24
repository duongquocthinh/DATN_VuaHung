using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio sources")]
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

        EnsureSfxSource();
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
        if (clip == null)
        {
            return;
        }

        EnsureSfxSource();
        if (sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    private void EnsureSfxSource()
    {
        if (sfxSource != null)
        {
            return;
        }

        AudioSource existingSource = GetComponent<AudioSource>();
        bool existingSourceLooksLikeMusic = existingSource != null
            && existingSource.clip != null
            && (existingSource.loop || existingSource.playOnAwake);

        if (existingSourceLooksLikeMusic)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 1f;
            return;
        }

        sfxSource = existingSource;
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 1f;
        }
    }
}
