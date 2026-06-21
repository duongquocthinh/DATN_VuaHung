using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float attackDistance = 4f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldown = 0.45f;

    private float nextAttackTime;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime || playerCamera == null)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, attackDistance, ~0, QueryTriggerInteraction.Collide))
        {
            AnimalHealth animal = hit.collider.GetComponentInParent<AnimalHealth>();

            if (animal != null)
            {
                animal.TakeDamage(damage);
            }
        }
    }
}
