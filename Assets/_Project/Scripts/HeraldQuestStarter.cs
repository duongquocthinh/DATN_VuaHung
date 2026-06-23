using UnityEngine;

public class HeraldQuestStarter : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Nguoi truyen lenh";
    [SerializeField] private string orderMessage = "Vua Hung mo cuoc thi dang banh. Moi nguoi hay bat dau cong viec!";
    [SerializeField] private VillageRoutineNPC[] villagers;

    private bool started;

    public string GetInteractionText()
    {
        return npcName;
    }

    public void Interact()
    {
        NotificationUI.ShowMessage(orderMessage, 4f);

        if (started)
        {
            return;
        }

        started = true;
        if (villagers == null)
        {
            return;
        }

        for (int i = 0; i < villagers.Length; i++)
        {
            if (villagers[i] != null)
            {
                villagers[i].BeginRoutine();
            }
        }
    }
}
