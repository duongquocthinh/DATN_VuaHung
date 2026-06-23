using System;
using System.Collections.Generic;
using UnityEngine;

public class ProcessingStation : MonoBehaviour, IInteractable
{
    [Serializable]
    public class RecipeItem
    {
        public string itemName;
        public int amount = 1;
    }

    [Serializable]
    public class StationRecipe
    {
        public string recipeName;
        public RecipeItem[] inputItems;
        public RecipeItem[] outputItems;
    }

    [SerializeField] private string stationName = "Cối xay gạo";

    [Header("Simple recipe")]
    [SerializeField] private string inputItemName = "Lúa";
    [SerializeField] private int inputAmount = 8;
    [SerializeField] private string outputItemName = "Gạo Nếp";
    [SerializeField] private int outputAmount = 4;

    [Header("Optional extra recipe items")]
    [SerializeField] private RecipeItem[] extraInputItems;
    [SerializeField] private RecipeItem[] extraOutputItems;

    [Header("Recipe UI")]
    [SerializeField] private bool useLargeRecipePanel;

    [Header("Numbered recipes")]
    [SerializeField] private StationRecipe[] recipes;

    public string StationName { get { return stationName; } }
    public bool HasNumberedRecipes { get { return recipes != null && recipes.Length > 0; } }
    public bool UseLargeRecipePanel { get { return useLargeRecipePanel; } }

    private void Awake()
    {
        SetupDefaultCookingRecipesIfNeeded();
    }

    public string GetInteractionText()
    {
        if (HasNumberedRecipes)
        {
            return GetNumberedRecipeText();
        }

        return "Cần: " + FormatItems(GetSimpleRequiredItems()) + "\n"
            + "Tạo: " + FormatItems(GetSimpleCreatedItems()) + "\n"
            + "Nhấn E để chế biến";
    }

    public void Interact()
    {
        if (HasNumberedRecipes)
        {
            NotificationUI.ShowMessage("Chọn phím 1 hoặc 2 để chế biến.");
            return;
        }

        ProcessRecipe(stationName, GetSimpleRequiredItems(), GetSimpleCreatedItems());
    }

    public void InteractRecipe(int recipeIndex)
    {
        if (!HasNumberedRecipes || recipeIndex < 0 || recipeIndex >= recipes.Length)
        {
            return;
        }

        StationRecipe recipe = recipes[recipeIndex];
        if (recipe == null)
        {
            return;
        }

        string recipeName = string.IsNullOrWhiteSpace(recipe.recipeName)
            ? "Công thức " + (recipeIndex + 1)
            : recipe.recipeName;

        ProcessRecipe(recipeName, GetItems(recipe.inputItems), GetItems(recipe.outputItems));
    }

    private void ProcessRecipe(string recipeName, List<RecipeItem> requiredItems, List<RecipeItem> createdItems)
    {
        if (InventorySystem.Instance == null)
        {
            NotificationUI.ShowMessage("Không tìm thấy InventorySystem.");
            return;
        }

        foreach (RecipeItem item in requiredItems)
        {
            if (!InventorySystem.Instance.HasItem(item.itemName, item.amount))
            {
                NotificationUI.ShowMessage("Bạn cần " + FormatItems(requiredItems) + " để tạo " + recipeName + ".");
                return;
            }
        }

        foreach (RecipeItem item in requiredItems)
        {
            InventorySystem.Instance.RemoveItem(item.itemName, item.amount);
        }

        foreach (RecipeItem item in createdItems)
        {
            InventorySystem.Instance.AddItem(item.itemName, item.amount);
        }

        PlayProcessSound();
        NotificationUI.ShowMessage("Đã tạo " + FormatItems(createdItems) + ".");
    }

    private void PlayProcessSound()
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        if (HasNumberedRecipes || LooksLikeCookingStation())
        {
            SoundManager.Instance.PlayCooking();
        }
        else
        {
            SoundManager.Instance.PlayGrinding();
        }
    }

    private string GetNumberedRecipeText()
    {
        List<string> lines = new List<string>();

        for (int i = 0; i < recipes.Length; i++)
        {
            StationRecipe recipe = recipes[i];
            if (recipe == null)
            {
                continue;
            }

            string recipeName = string.IsNullOrWhiteSpace(recipe.recipeName)
                ? "Công thức " + (i + 1)
                : recipe.recipeName;

            if (lines.Count > 0)
            {
                lines.Add("");
            }

            lines.Add("[" + (i + 1) + "] " + recipeName);
            lines.Add("Cần: " + FormatItems(GetItems(recipe.inputItems)));
            lines.Add("Tạo: " + FormatItems(GetItems(recipe.outputItems)));
        }

        lines.Add("");
        lines.Add("Nhấn phím số để chế biến");
        return string.Join("\n", lines);
    }

    private void SetupDefaultCookingRecipesIfNeeded()
    {
        if (!LooksLikeCookingStation())
        {
            return;
        }

        stationName = "Nồi nấu bánh";
        useLargeRecipePanel = true;

        if (HasNumberedRecipes)
        {
            return;
        }

        recipes = new StationRecipe[]
        {
            new StationRecipe
            {
                recipeName = "Bánh Chưng",
                inputItems = new RecipeItem[]
                {
                    new RecipeItem { itemName = "Lá Dong", amount = 2 },
                    new RecipeItem { itemName = "Gạo Nếp", amount = 4 },
                    new RecipeItem { itemName = "Đậu Xanh", amount = 2 },
                    new RecipeItem { itemName = "Thịt Lợn", amount = 1 }
                },
                outputItems = new RecipeItem[]
                {
                    new RecipeItem { itemName = "Bánh Chưng", amount = 1 }
                }
            },
            new StationRecipe
            {
                recipeName = "Bánh Giầy",
                inputItems = new RecipeItem[]
                {
                    new RecipeItem { itemName = "Gạo Nếp", amount = 4 }
                },
                outputItems = new RecipeItem[]
                {
                    new RecipeItem { itemName = "Bánh Giầy", amount = 1 }
                }
            }
        };
    }

    private bool LooksLikeCookingStation()
    {
        string sourceName = (stationName + " " + gameObject.name).ToLowerInvariant();
        return sourceName.Contains("cooking")
            || sourceName.Contains("stove")
            || sourceName.Contains("nồi")
            || sourceName.Contains("noi")
            || sourceName.Contains("nấu")
            || sourceName.Contains("nau")
            || sourceName.Contains("bánh")
            || sourceName.Contains("banh");
    }

    private List<RecipeItem> GetSimpleRequiredItems()
    {
        List<RecipeItem> items = new List<RecipeItem>();
        AddRecipeItem(items, inputItemName, inputAmount);
        AddRecipeItems(items, extraInputItems);
        return items;
    }

    private List<RecipeItem> GetSimpleCreatedItems()
    {
        List<RecipeItem> items = new List<RecipeItem>();
        AddRecipeItem(items, outputItemName, outputAmount);
        AddRecipeItems(items, extraOutputItems);
        return items;
    }

    private List<RecipeItem> GetItems(RecipeItem[] sourceItems)
    {
        List<RecipeItem> items = new List<RecipeItem>();
        AddRecipeItems(items, sourceItems);
        return items;
    }

    private void AddRecipeItems(List<RecipeItem> items, RecipeItem[] sourceItems)
    {
        if (sourceItems == null)
        {
            return;
        }

        foreach (RecipeItem item in sourceItems)
        {
            if (item == null)
            {
                continue;
            }

            AddRecipeItem(items, item.itemName, item.amount);
        }
    }

    private void AddRecipeItem(List<RecipeItem> items, string itemName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
        {
            return;
        }

        foreach (RecipeItem item in items)
        {
            if (InventorySystem.NormalizeItemName(item.itemName) == InventorySystem.NormalizeItemName(itemName))
            {
                item.amount += amount;
                return;
            }
        }

        items.Add(new RecipeItem { itemName = itemName, amount = amount });
    }

    private string FormatItems(List<RecipeItem> items)
    {
        List<string> parts = new List<string>();

        foreach (RecipeItem item in items)
        {
            parts.Add(item.itemName + " x" + item.amount);
        }

        return parts.Count > 0 ? string.Join(" + ", parts) : "Không có";
    }
}
