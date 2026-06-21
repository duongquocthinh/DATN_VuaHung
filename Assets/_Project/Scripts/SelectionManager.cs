using UnityEngine;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    [Header("Item name UI")]
    public GameObject interaction_Info_UI;
    public TextMeshProUGUI interaction_text;

    [Header("Recipe UI")]
    public GameObject recipe_Info_UI;
    public TextMeshProUGUI recipe_text;

    [SerializeField] private float interactionDistance = 10f;

    private void Start()
    {
        if (interaction_text == null && interaction_Info_UI != null)
        {
            interaction_text = interaction_Info_UI.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (recipe_text == null && recipe_Info_UI != null)
        {
            recipe_text = recipe_Info_UI.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        HideItemUI();
        HideRecipeUI();
    }

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            interactionDistance,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (RaycastHit hit in hits)
        {
            IInteractable interactable =
                hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                if (interactable is ProcessingStation)
                {
                    HideItemUI();
                    ShowRecipeUI(interactable.GetInteractionText());
                }
                else
                {
                    HideRecipeUI();
                    ShowItemUI(interactable.GetInteractionText());
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                    HideItemUI();
                    HideRecipeUI();
                }

                return;
            }
        }

        HideItemUI();
        HideRecipeUI();
    }

    private void ShowItemUI(string message)
    {
        if (interaction_Info_UI == null || interaction_text == null)
        {
            return;
        }

        interaction_text.text = message;
        interaction_Info_UI.SetActive(true);
    }

    private void HideItemUI()
    {
        if (interaction_Info_UI != null)
        {
            interaction_Info_UI.SetActive(false);
        }
    }

    private void ShowRecipeUI(string message)
    {
        if (recipe_Info_UI == null || recipe_text == null)
        {
            return;
        }

        recipe_text.text = message;
        recipe_Info_UI.SetActive(true);
    }

    private void HideRecipeUI()
    {
        if (recipe_Info_UI != null)
        {
            recipe_Info_UI.SetActive(false);
        }
    }
}
