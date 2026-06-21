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

    [SerializeField] private string stationName = "Cối xay gạo";
    [SerializeField] private string inputItemName = "Lúa";
    [SerializeField] private int inputAmount = 8;
    [SerializeField] private string outputItemName = "Gạo Nếp";
    [SerializeField] private int outputAmount = 4;

    [Header("Optional extra recipe items")]
    [SerializeField] private RecipeItem[] extraInputItems;
    [SerializeField] private RecipeItem[] extraOutputItems;

    public string GetInteractionText()
    {
        return stationName + "\n"
            + "Cần: " + FormatItems(GetRequiredItems()) + "\n"
            + "Tạo: " + FormatItems(GetCreatedItems()) + "\n"
            + "Nhấn E để chế tạo";
    }

    public void Interact()
    {
        if (InventorySystem.Instance == null)
        {
            NotificationUI.ShowMessage("Không tìm thấy InventorySystem.");
            return;
        }

        List<RecipeItem> requiredItems = GetRequiredItems();

        foreach (RecipeItem item in requiredItems)
        {
            if (!InventorySystem.Instance.HasItem(item.itemName, item.amount))
            {
                NotificationUI.ShowMessage("Bạn cần " + FormatItems(requiredItems) + " để tạo " + stationName + ".");
                return;
            }
        }

        foreach (RecipeItem item in requiredItems)
        {
            InventorySystem.Instance.RemoveItem(item.itemName, item.amount);
        }

        List<RecipeItem> createdItems = GetCreatedItems();

        foreach (RecipeItem item in createdItems)
        {
            InventorySystem.Instance.AddItem(item.itemName, item.amount);
        }

        NotificationUI.ShowMessage("Đã tạo " + FormatItems(createdItems) + ".");
    }

    private List<RecipeItem> GetRequiredItems()
    {
        List<RecipeItem> items = new List<RecipeItem>();
        AddRecipeItem(items, inputItemName, inputAmount);
        AddRecipeItems(items, extraInputItems);
        return items;
    }

    private List<RecipeItem> GetCreatedItems()
    {
        List<RecipeItem> items = new List<RecipeItem>();
        AddRecipeItem(items, outputItemName, outputAmount);
        AddRecipeItems(items, extraOutputItems);
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
