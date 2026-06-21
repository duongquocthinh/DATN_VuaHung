using UnityEngine;

public class AnimalHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private string fallbackItemName = "Thit Lon";
    [SerializeField] private int fallbackAmount = 1;
    [SerializeField] private float dropForwardOffset = 0.6f;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= Mathf.Max(1, damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (dropPrefab != null)
        {
            Vector3 dropPosition = transform.position + transform.forward * dropForwardOffset + Vector3.up * 0.4f;
            Instantiate(dropPrefab, dropPosition, Quaternion.identity);
        }
        else if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddItem(fallbackItemName, fallbackAmount);
            NotificationUI.ShowMessage("Da nhan " + fallbackItemName + " x" + fallbackAmount + ".");
        }

        Destroy(gameObject);
    }
}
