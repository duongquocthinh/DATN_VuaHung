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
        Debug.Log(ItemName + " added to inventory");
        Destroy(gameObject);
    }
}