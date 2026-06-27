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
    [SerializeField] private float maxCookingSoundDuration = 2.5f;

    private AudioSource cookingSource;

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
        PlayCookingOneShot();
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

    private void PlayCookingOneShot()
    {
        if (cookingSound == null)
        {
            return;
        }

        EnsureCookingSource();
        if (cookingSource == null)
        {
            PlayOneShot(cookingSound);
            return;
        }

        cookingSource.Stop();
        cookingSource.clip = cookingSound;
        cookingSource.loop = false;
        cookingSource.Play();

        CancelInvoke(nameof(StopCookingSound));
        if (maxCookingSoundDuration > 0f)
        {
            Invoke(nameof(StopCookingSound), Mathf.Min(maxCookingSoundDuration, cookingSound.length));
        }
    }

    private void StopCookingSound()
    {
        if (cookingSource != null)
        {
            cookingSource.Stop();
        }
    }

    private void EnsureCookingSource()
    {
        if (cookingSource != null)
        {
            return;
        }

        cookingSource = gameObject.AddComponent<AudioSource>();
        cookingSource.playOnAwake = false;
        cookingSource.volume = 1f;
    }
}
