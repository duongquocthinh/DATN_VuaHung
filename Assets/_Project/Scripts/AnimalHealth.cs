using System.Collections;
using UnityEngine;

public class AnimalHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private string fallbackItemName = "Thit Lon";
    [SerializeField] private int fallbackAmount = 3;
    [SerializeField] private float dropRadius = 0.9f;
    [SerializeField] private float deathDelay = 1.1f;
    [SerializeField] private float dropGroundOffset = 0.18f;

    [Header("Respawn")]
    [SerializeField] private bool respawnAfterDeath;
    [SerializeField] private float respawnDelay = 45f;

    private int currentHealth;
    private bool isDead;
    private Animator animator;
    private Collider[] colliders;
    private Renderer[] renderers;
    private AudioSource[] audioSources;
    private SimpleAnimalWander wander;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        animator = GetComponentInChildren<Animator>();
        colliders = GetComponentsInChildren<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
        audioSources = GetComponentsInChildren<AudioSource>();
        wander = GetComponent<SimpleAnimalWander>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    private void Start()
    {
        RestartLoopingAudio();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= Mathf.Max(1, damage);

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        PlayTrigger("Hit1");
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;

        if (wander != null)
        {
            wander.StopWandering();
        }

        foreach (Collider animalCollider in colliders)
        {
            animalCollider.enabled = false;
        }

        PlayTrigger("Death1");
        yield return new WaitForSeconds(deathDelay);

        SpawnDrops();

        if (respawnAfterDeath)
        {
            SetAnimalVisible(false);
            StopLoopingAudio();
            yield return new WaitForSeconds(Mathf.Max(0.1f, respawnDelay));
            Respawn();
            yield break;
        }

        Destroy(gameObject);
    }

    private void Respawn()
    {
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        currentHealth = Mathf.Max(1, maxHealth);
        isDead = false;

        SetAnimalVisible(true);

        foreach (Collider animalCollider in colliders)
        {
            if (animalCollider != null)
            {
                animalCollider.enabled = true;
            }
        }

        RestartLoopingAudio();

        if (wander != null)
        {
            wander.ResumeWandering();
        }

        PlayTrigger("Idle");
    }

    private void SetAnimalVisible(bool visible)
    {
        foreach (Renderer animalRenderer in renderers)
        {
            if (animalRenderer != null)
            {
                animalRenderer.enabled = visible;
            }
        }
    }

    private void StopLoopingAudio()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null && audioSource.loop)
            {
                audioSource.Stop();
            }
        }
    }

    private void RestartLoopingAudio()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null && audioSource.loop && audioSource.playOnAwake)
            {
                audioSource.Play();
            }
        }
    }

    private void SpawnDrops()
    {
        int amount = Mathf.Max(1, fallbackAmount);

        for (int i = 0; i < amount; i++)
        {
            Vector3 dropPosition = GetDropPosition();
            GameObject drop = dropPrefab != null
                ? Instantiate(dropPrefab, dropPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f))
                : CreateDefaultMeatDrop(dropPosition);

            SetupPickupDrop(drop);
        }
    }

    private Vector3 GetDropPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
        Vector3 rayStart = transform.position + new Vector3(randomOffset.x, 2f, randomOffset.y);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit groundHit, 8f, ~0, QueryTriggerInteraction.Ignore))
        {
            return groundHit.point + Vector3.up * dropGroundOffset;
        }

        return transform.position + new Vector3(randomOffset.x, dropGroundOffset, randomOffset.y);
    }

    private GameObject CreateDefaultMeatDrop(Vector3 position)
    {
        GameObject meat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        meat.name = "Thit_Lon_Drop";
        meat.transform.position = position;
        meat.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        meat.transform.localScale = new Vector3(0.75f, 0.22f, 0.45f);

        Renderer renderer = meat.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = new Color(0.85f, 0.28f, 0.24f);
            renderer.material = material;
        }

        return meat;
    }

    private void SetupPickupDrop(GameObject drop)
    {
        if (drop == null)
        {
            return;
        }

        drop.name = "Thit_Lon_Drop";
        drop.SetActive(true);

        InteractableObject interactable = drop.GetComponent<InteractableObject>();
        if (interactable == null)
        {
            interactable = drop.AddComponent<InteractableObject>();
        }

        interactable.ItemName = fallbackItemName;
        interactable.Amount = 1;

        Collider dropCollider = drop.GetComponent<Collider>();
        if (dropCollider == null)
        {
            dropCollider = drop.AddComponent<BoxCollider>();
        }

        dropCollider.isTrigger = true;

        Rigidbody dropRigidbody = drop.GetComponent<Rigidbody>();
        if (dropRigidbody != null)
        {
            dropRigidbody.isKinematic = true;
            dropRigidbody.useGravity = false;
        }
    }

    private void PlayTrigger(string triggerName)
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Run");
        animator.ResetTrigger("Hit1");
        animator.ResetTrigger("Hit2");
        animator.ResetTrigger("Death1");
        animator.ResetTrigger("Death2");
        animator.SetTrigger(triggerName);
    }
}
