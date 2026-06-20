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
            if (InventorySystem.Instance.AddItem(ItemName))
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning("InventorySystem is missing in the scene.");
        }
    }
}
