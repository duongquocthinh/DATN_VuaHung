using UnityEngine;

public class QuestTurnInNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Vua Hung";
    [SerializeField] private string requiredFirstItem = "Banh Chung";
    [SerializeField] private int requiredFirstAmount = 1;
    [SerializeField] private string requiredSecondItem = "Banh Giay";
    [SerializeField] private int requiredSecondAmount = 1;
    [SerializeField] private bool removeItemsOnComplete = true;

    private bool questCompleted;

    public string GetInteractionText()
    {
        return npcName;
    }

    public void Interact()
    {
        if (questCompleted)
        {
            NotificationUI.ShowMessage("Nhiem vu da hoan thanh. Lang Lieu da dang banh cho Vua Hung.", 4f);
            return;
        }

        if (InventorySystem.Instance == null)
        {
            NotificationUI.ShowMessage("Khong tim thay InventorySystem.");
            return;
        }

        bool hasFirstItem = InventorySystem.Instance.HasItem(requiredFirstItem, requiredFirstAmount);
        bool hasSecondItem = InventorySystem.Instance.HasItem(requiredSecondItem, requiredSecondAmount);

        if (!hasFirstItem || !hasSecondItem)
        {
            NotificationUI.ShowMessage(
                "Can " + requiredFirstItem + " x" + requiredFirstAmount
                + " va " + requiredSecondItem + " x" + requiredSecondAmount
                + " de dang len Vua Hung.",
                4f
            );
            return;
        }

        if (removeItemsOnComplete)
        {
            InventorySystem.Instance.RemoveItem(requiredFirstItem, requiredFirstAmount);
            InventorySystem.Instance.RemoveItem(requiredSecondItem, requiredSecondAmount);
        }

        questCompleted = true;
        NotificationUI.ShowMessage("Hoan thanh! Vua Hung da chap nhan Banh Chung va Banh Giay.", 5f);
    }
}
