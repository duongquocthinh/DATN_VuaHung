using System.Collections;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    public string ItemName;
    public int Amount = 1;

    [Header("Respawn")]
    [SerializeField] private bool respawnAfterPickup;
    [SerializeField] private float respawnDelay = 30f;

    private bool isPickedUp;

    public string GetItemName()
    {
        return ItemName;
    }

    public string GetInteractionText()
    {
        return ItemName;
    }

    public void Interact()
    {
        PickUp();
    }

    public void PickUp()
    {
        if (isPickedUp)
        {
            return;
        }

        if (InventorySystem.Instance != null)
        {
            int pickupAmount = Mathf.Max(1, Amount);

            if (InventorySystem.Instance.AddItem(ItemName, pickupAmount))
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayPickup();
                }

                NotificationUI.ShowMessage("Đã nhặt " + ItemName + " x" + pickupAmount + ".", 2f);

                if (respawnAfterPickup)
                {
                    StartCoroutine(RespawnRoutine());
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                NotificationUI.ShowMessage("Túi đồ đã đầy.", 2f);
            }
        }
        else
        {
            Debug.LogWarning("InventorySystem is missing in the scene.");
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isPickedUp = true;
        SetPickupVisible(false);

        yield return new WaitForSeconds(Mathf.Max(0.1f, respawnDelay));

        SetPickupVisible(true);
        isPickedUp = false;
    }

    private void SetPickupVisible(bool visible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer itemRenderer in renderers)
        {
            itemRenderer.enabled = visible;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider itemCollider in colliders)
        {
            itemCollider.enabled = visible;
        }
    }
}
