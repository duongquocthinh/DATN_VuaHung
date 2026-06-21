using UnityEngine;

public class ProcessingStation : MonoBehaviour, IInteractable
{
    [SerializeField] private string stationName = "Cối xay gạo";
    [SerializeField] private string inputItemName = "Lúa";
    [SerializeField] private int inputAmount = 8;
    [SerializeField] private string outputItemName = "Gạo nếp";
    [SerializeField] private int outputAmount = 4;

    public string GetInteractionText()
    {
        return stationName + "\n"
            + "Cần: " + inputItemName + " x" + inputAmount + "\n"
            + "Tạo: " + outputItemName + " x" + outputAmount + "\n"
            + "Nhấn E để chế biến";
    }

    public void Interact()
    {
        if (InventorySystem.Instance == null)
        {
            NotificationUI.ShowMessage("Không tìm thấy InventorySystem.");
            return;
        }

        if (!InventorySystem.Instance.HasItem(inputItemName, inputAmount))
        {
            NotificationUI.ShowMessage("Bạn cần " + inputItemName + " x" + inputAmount + " để tạo " + outputItemName + ".");
            return;
        }

        if (InventorySystem.Instance.RemoveItem(inputItemName, inputAmount))
        {
            InventorySystem.Instance.AddItem(outputItemName, outputAmount);
            NotificationUI.ShowMessage("Đã tạo " + outputItemName + " x" + outputAmount + ".");
        }
    }
}
