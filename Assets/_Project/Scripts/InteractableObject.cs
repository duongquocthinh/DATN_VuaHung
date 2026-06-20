using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string ItemName;
    public int Amount = 1;

    public string GetItemName()
    {
        return ItemName;
    }

    public void PickUp()
    {
        if (InventorySystem.Instance != null)
        {
            if (InventorySystem.Instance.AddItem(ItemName, Mathf.Max(1, Amount)))
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
