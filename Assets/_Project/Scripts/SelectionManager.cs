using UnityEngine;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    [Header("Item name UI")]
    public GameObject interaction_Info_UI;
    public TextMeshProUGUI interaction_text;

    [Header("Recipe UI")]
    public GameObject recipe_Info_UI;
    public TextMeshProUGUI recipe_title_text;
    public TextMeshProUGUI recipe_text;

    [Header("Large Recipe UI")]
    public GameObject large_recipe_Info_UI;
    public TextMeshProUGUI large_recipe_title_text;
    public TextMeshProUGUI large_recipe_text;

    [SerializeField] private float interactionDistance = 10f;

    private static readonly Vector2 SmallRecipePanelSize = new Vector2(420f, 190f);
    private static readonly Vector2 LargeRecipePanelSize = new Vector2(560f, 360f);
    private static readonly Vector2 SmallRecipeTextSize = new Vector2(340f, 110f);
    private static readonly Vector2 LargeRecipeTextSize = new Vector2(470f, 270f);
    private static readonly Vector2 SmallTitlePanelSize = new Vector2(190f, 48f);
    private static readonly Vector2 LargeTitlePanelSize = new Vector2(230f, 48f);

    private void Start()
    {
        if (interaction_text == null)
        {
            interaction_text = FindText(interaction_Info_UI, "ItemNameText", true);
        }

        if (recipe_text == null)
        {
            recipe_text = FindText(recipe_Info_UI, "interaction_info_UI", true);
        }

        if (recipe_title_text == null)
        {
            recipe_title_text = FindText(recipe_Info_UI, "InteractionTitle", false);
        }

        if (large_recipe_text == null)
        {
            large_recipe_text = FindText(large_recipe_Info_UI, "interaction_info_UI", true);
        }

        if (large_recipe_title_text == null)
        {
            large_recipe_title_text = FindText(large_recipe_Info_UI, "InteractionTitle", false);
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
            ProcessingStation processingStation = hit.collider.GetComponentInParent<ProcessingStation>();

            if (processingStation != null)
            {
                HideItemUI();
                ShowRecipeUI(
                    processingStation.StationName,
                    processingStation.GetInteractionText(),
                    processingStation.UseLargeRecipePanel
                );
                HandleProcessingStationInput(processingStation);

                return;
            }

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                HideRecipeUI();
                ShowItemUI(interactable.GetInteractionText());

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
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 2;
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 3;
        return -1;
    }

    private void ShowItemUI(string message)
    {
        if (interaction_text != null)
        {
            interaction_text.text = message;
        }

        if (interaction_Info_UI != null)
        {
            interaction_Info_UI.SetActive(true);
        }
    }

    private void HideItemUI()
    {
        if (interaction_Info_UI != null)
        {
            interaction_Info_UI.SetActive(false);
        }
    }

    private void ShowRecipeUI(string title, string message, bool useLargePanel)
    {
        GameObject activePanel = useLargePanel && large_recipe_Info_UI != null
            ? large_recipe_Info_UI
            : recipe_Info_UI;

        TextMeshProUGUI activeTitleText = useLargePanel && large_recipe_title_text != null
            ? large_recipe_title_text
            : recipe_title_text;

        TextMeshProUGUI activeRecipeText = useLargePanel && large_recipe_text != null
            ? large_recipe_text
            : recipe_text;

        if (activePanel == null)
        {
            return;
        }

        if (recipe_Info_UI != null && recipe_Info_UI != activePanel)
        {
            recipe_Info_UI.SetActive(false);
        }

        if (large_recipe_Info_UI != null && large_recipe_Info_UI != activePanel)
        {
            large_recipe_Info_UI.SetActive(false);
        }

        ApplyRecipePanelLayout(activePanel, activeTitleText, activeRecipeText, useLargePanel);

        if (activeTitleText != null)
        {
            activeTitleText.text = title;
        }

        if (activeRecipeText != null)
        {
            activeRecipeText.text = message;
        }

        activePanel.SetActive(true);
    }

    private void ApplyRecipePanelLayout(
        GameObject panel,
        TextMeshProUGUI titleText,
        TextMeshProUGUI bodyText,
        bool useLargePanel)
    {
        if (panel == null)
        {
            return;
        }

        Vector2 panelSize = useLargePanel ? LargeRecipePanelSize : SmallRecipePanelSize;
        Vector2 textSize = useLargePanel ? LargeRecipeTextSize : SmallRecipeTextSize;
        Vector2 titleSize = useLargePanel ? LargeTitlePanelSize : SmallTitlePanelSize;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = panelSize;
        }

        RectTransform titlePanelRect = null;
        if (titleText != null && titleText.transform.parent != null)
        {
            titlePanelRect = titleText.transform.parent.GetComponent<RectTransform>();
        }

        if (titlePanelRect != null)
        {
            titlePanelRect.sizeDelta = titleSize;
            titlePanelRect.anchoredPosition = new Vector2(0f, panelSize.y * 0.5f + titleSize.y * 0.5f - 2f);
        }

        RectTransform titleTextRect = titleText != null ? titleText.GetComponent<RectTransform>() : null;
        if (titleTextRect != null)
        {
            titleTextRect.sizeDelta = titleSize;
            titleTextRect.anchoredPosition = Vector2.zero;
        }

        if (titleText != null)
        {
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 16f;
            titleText.fontSizeMax = 24f;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        RectTransform bodyTextRect = bodyText != null ? bodyText.GetComponent<RectTransform>() : null;
        if (bodyTextRect != null)
        {
            bodyTextRect.sizeDelta = textSize;
            bodyTextRect.anchoredPosition = useLargePanel
                ? new Vector2(0f, -10f)
                : new Vector2(0f, -8f);
        }

        if (bodyText != null)
        {
            bodyText.enableAutoSizing = true;
            bodyText.fontSizeMin = useLargePanel ? 15f : 16f;
            bodyText.fontSizeMax = useLargePanel ? 22f : 23f;
            bodyText.alignment = TextAlignmentOptions.MidlineLeft;
        }
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

    private TextMeshProUGUI FindText(GameObject root, string objectName, bool allowFallback)
    {
        if (root == null)
        {
            return null;
        }

        TextMeshProUGUI found = FindTextInChildren(root.transform, objectName);
        if (found != null)
        {
            return found;
        }

        if (root.transform.parent != null)
        {
            found = FindTextInChildren(root.transform.parent, objectName);
            if (found != null)
            {
                return found;
            }
        }

        if (!allowFallback)
        {
            return null;
        }

        TextMeshProUGUI[] texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        return texts.Length > 0 ? texts[0] : null;
    }

    private TextMeshProUGUI FindTextInChildren(Transform root, string objectName)
    {
        TextMeshProUGUI[] texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name == objectName)
            {
                return text;
            }
        }

        return null;
    }
}
