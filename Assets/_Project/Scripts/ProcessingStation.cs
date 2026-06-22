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

    [SerializeField] private string stationName = "Coi xay gao";

    [Header("Simple recipe")]
    [SerializeField] private string inputItemName = "Lua";
    [SerializeField] private int inputAmount = 8;
    [SerializeField] private string outputItemName = "Gao Nep";
    [SerializeField] private int outputAmount = 4;

    [Header("Optional extra recipe items")]
    [SerializeField] private RecipeItem[] extraInputItems;
    [SerializeField] private RecipeItem[] extraOutputItems;

    [Header("Recipe UI")]
    [SerializeField] private bool useLargeRecipePanel;

    [Header("Numbered recipes")]
    [SerializeField] private StationRecipe[] recipes;

    public bool HasNumberedRecipes
    {
        get { return recipes != null && recipes.Length > 0; }
    }

    public bool UseLargeRecipePanel
    {
        get { return useLargeRecipePanel; }
    }

    public string GetInteractionText()
    {
        if (HasNumberedRecipes)
        {
            return GetNumberedRecipeText();
        }

        return stationName + "\n"
            + "Can: " + FormatItems(GetSimpleRequiredItems()) + "\n"
            + "Tao: " + FormatItems(GetSimpleCreatedItems()) + "\n"
            + "Nhan E de che bien";
    }

    public void Interact()
    {
        if (HasNumberedRecipes)
        {
            NotificationUI.ShowMessage("Chon phim 1 hoac 2 de che bien.");
            return;
        }

        ProcessRecipe(stationName, GetSimpleRequiredItems(), GetSimpleCreatedItems());
    }

    public void InteractRecipe(int recipeIndex)
    {
        if (!HasNumberedRecipes)
        {
            Interact();
            return;
        }

        if (recipeIndex < 0 || recipeIndex >= recipes.Length || recipes[recipeIndex] == null)
        {
            NotificationUI.ShowMessage("Cong thuc khong hop le.");
            return;
        }

        StationRecipe recipe = recipes[recipeIndex];
        string recipeName = string.IsNullOrWhiteSpace(recipe.recipeName)
            ? "Cong thuc " + (recipeIndex + 1)
            : recipe.recipeName;

        ProcessRecipe(recipeName, GetItems(recipe.inputItems), GetItems(recipe.outputItems));
    }

    private void ProcessRecipe(string recipeName, List<RecipeItem> requiredItems, List<RecipeItem> createdItems)
    {
        if (InventorySystem.Instance == null)
        {
            NotificationUI.ShowMessage("Khong tim thay InventorySystem.");
            return;
        }

        if (requiredItems.Count == 0)
        {
            NotificationUI.ShowMessage("Cong thuc chua co nguyen lieu.");
            return;
        }

        if (createdItems.Count == 0)
        {
            NotificationUI.ShowMessage("Cong thuc chua co vat pham tao ra.");
            return;
        }

        foreach (RecipeItem item in requiredItems)
        {
            if (!InventorySystem.Instance.HasItem(item.itemName, item.amount))
            {
                NotificationUI.ShowMessage("Ban can " + FormatItems(requiredItems) + " de tao " + recipeName + ".");
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

        NotificationUI.ShowMessage("Da tao " + FormatItems(createdItems) + ".");
    }

    private string GetNumberedRecipeText()
    {
        List<string> lines = new List<string>();
        lines.Add(stationName);

        for (int i = 0; i < recipes.Length; i++)
        {
            StationRecipe recipe = recipes[i];
            if (recipe == null)
            {
                continue;
            }

            string recipeName = string.IsNullOrWhiteSpace(recipe.recipeName)
                ? "Cong thuc " + (i + 1)
                : recipe.recipeName;

            lines.Add("[" + (i + 1) + "] " + recipeName);
            lines.Add("Can: " + FormatItems(GetItems(recipe.inputItems)));
            lines.Add("Tao: " + FormatItems(GetItems(recipe.outputItems)));
        }

        lines.Add("Nhan phim so de che bien");
        return string.Join("\n", lines);
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

    private List<RecipeItem> GetItems(RecipeItem[] recipeItems)
    {
        List<RecipeItem> items = new List<RecipeItem>();
        AddRecipeItems(items, recipeItems);
        return items;
    }

    private void AddRecipeItems(List<RecipeItem> items, RecipeItem[] recipeItems)
    {
        if (recipeItems == null)
        {
            return;
        }

        foreach (RecipeItem recipeItem in recipeItems)
        {
            if (recipeItem != null)
            {
                AddRecipeItem(items, recipeItem.itemName, recipeItem.amount);
            }
        }
    }

    private void AddRecipeItem(List<RecipeItem> items, string itemName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return;
        }

        items.Add(new RecipeItem
        {
            itemName = itemName,
            amount = Mathf.Max(1, amount)
        });
    }

    private string FormatItems(List<RecipeItem> items)
    {
        if (items == null || items.Count == 0)
        {
            return "Khong co";
        }

        List<string> itemTexts = new List<string>();

        foreach (RecipeItem item in items)
        {
            itemTexts.Add(item.itemName + " x" + item.amount);
        }

        return string.Join(" + ", itemTexts);
    }
}
