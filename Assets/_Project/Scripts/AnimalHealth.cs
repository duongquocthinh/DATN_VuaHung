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

    private int currentHealth;
    private bool isDead;
    private Animator animator;
    private Collider[] colliders;
    private SimpleAnimalWander wander;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        animator = GetComponentInChildren<Animator>();
        colliders = GetComponentsInChildren<Collider>();
        wander = GetComponent<SimpleAnimalWander>();
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
        Destroy(gameObject);
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
