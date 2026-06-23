using UnityEngine;

public class NPCDialogue : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private string[] dialogueLines =
    {
        "Hay thu thap nguyen lieu de lam banh dang Vua Hung."
    };

    private int dialogueIndex;

    public string GetInteractionText()
    {
        return npcName;
    }

    public void Interact()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            NotificationUI.ShowMessage(npcName);
            return;
        }

        string line = dialogueLines[dialogueIndex];
        NotificationUI.ShowMessage(line, 4f);
        dialogueIndex = (dialogueIndex + 1) % dialogueLines.Length;
    }
}
