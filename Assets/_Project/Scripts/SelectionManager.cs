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

    [Header("Large Recipe UI")]
    public GameObject large_recipe_Info_UI;
    public TextMeshProUGUI large_recipe_text;

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

        if (large_recipe_text == null && large_recipe_Info_UI != null)
        {
            large_recipe_text = large_recipe_Info_UI.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        HideItemUI();
        HideRecipeUI();
    }

    private void Update()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            HideItemUI();
            HideRecipeUI();
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            interactionDistance,
            ~0,
            QueryTriggerInteraction.Collide
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            IInteractable interactable =
                hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                ProcessingStation processingStation = interactable as ProcessingStation;

                if (processingStation != null)
                {
                    HideItemUI();
                    ShowRecipeUI(processingStation.GetInteractionText(), processingStation.UseLargeRecipePanel);
                    HandleProcessingStationInput(processingStation);
                }
                else
                {
                    HideRecipeUI();
                    ShowItemUI(interactable.GetInteractionText());

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactable.Interact();
                        HideItemUI();
                        HideRecipeUI();
                    }
                }

                return;
            }
        }

        HideItemUI();
        HideRecipeUI();
    }

    private void HandleProcessingStationInput(ProcessingStation processingStation)
    {
        if (processingStation.HasNumberedRecipes)
        {
            int recipeIndex = GetPressedRecipeIndex();

            if (recipeIndex >= 0)
            {
                processingStation.InteractRecipe(recipeIndex);
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            processingStation.Interact();
        }
    }

    private int GetPressedRecipeIndex()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            return 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            return 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            return 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            return 3;
        }

        return -1;
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

    private void ShowRecipeUI(string message, bool useLargePanel)
    {
        HideRecipeUI();

        if (useLargePanel && large_recipe_Info_UI != null && large_recipe_text != null)
        {
            large_recipe_text.text = message;
            large_recipe_Info_UI.SetActive(true);
            return;
        }

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

        if (large_recipe_Info_UI != null)
        {
            large_recipe_Info_UI.SetActive(false);
        }
    }
}
