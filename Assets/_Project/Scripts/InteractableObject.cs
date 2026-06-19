using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string ItemName;

    public string GetItemName()
    {
        return ItemName;
    }

    public void PickUp()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddItem(ItemName);
        }
        else
        {
            Debug.LogWarning("InventorySystem is missing in the scene.");
        }

        Destroy(gameObject);
    }
}
