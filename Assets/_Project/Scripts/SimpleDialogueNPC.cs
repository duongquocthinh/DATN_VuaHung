using UnityEngine;

public class SimpleDialogueNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Vua Hung";
    [TextArea(2, 4)]
    [SerializeField] private string dialogueLine = "Lang Lieu, hay chuan bi le vat that y nghia de dang len to tien.";
    [SerializeField] private float messageDuration = 4f;

    public string GetInteractionText()
    {
        return npcName;
    }

    public void Interact()
    {
        if (string.IsNullOrWhiteSpace(dialogueLine))
        {
            return;
        }

        NotificationUI.ShowMessage(dialogueLine, messageDuration);
    }
}
