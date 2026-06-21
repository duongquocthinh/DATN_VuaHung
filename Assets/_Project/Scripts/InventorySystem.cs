using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Serializable]
    public class InventoryItemIcon
    {
        public string itemName;
        public Sprite icon;
    }

    public static InventorySystem Instance { get; private set; }

    public GameObject inventoryScreenUI;
    public InventorySlotUI[] itemSlots;
    public InventoryItemIcon[] itemIcons;
    public bool isOpen;

    private readonly Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    private readonly List<string> itemOrder = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        isOpen = false;

        if (inventoryScreenUI != null)
        {
            inventoryScreenUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryScreenUI != null)
        {
            inventoryScreenUI.SetActive(isOpen);
        }
    }

    public bool AddItem(string itemName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return false;
        }

        amount = Mathf.Max(1, amount);
        string storedItemName = GetStoredItemName(itemName);

        if (storedItemName == null)
        {
            if (IsFull())
            {
                Debug.Log("Inventory is full.");
                return false;
            }

            storedItemName = itemName;
            itemCounts[storedItemName] = 0;
            itemOrder.Add(storedItemName);
        }

        itemCounts[storedItemName] += amount;
        RefreshInventoryUI();
        Debug.Log(storedItemName + " x" + itemCounts[storedItemName]);
        return true;
    }

    public bool RemoveItem(string itemName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return false;
        }

        amount = Mathf.Max(1, amount);
        string storedItemName = GetStoredItemName(itemName);

        if (storedItemName == null || itemCounts[storedItemName] < amount)
        {
            return false;
        }

        itemCounts[storedItemName] -= amount;

        if (itemCounts[storedItemName] <= 0)
        {
            itemCounts.Remove(storedItemName);
            itemOrder.Remove(storedItemName);
        }

        RefreshInventoryUI();
        return true;
    }

    public bool HasItem(string itemName, int amount = 1)
    {
        string storedItemName = GetStoredItemName(itemName);
        return storedItemName != null && itemCounts[storedItemName] >= amount;
    }

    public bool IsFull()
    {
        return itemSlots != null && itemOrder.Count >= itemSlots.Length;
    }

    private void RefreshInventoryUI()
    {
        if (itemSlots == null)
        {
            return;
        }

        foreach (InventorySlotUI slot in itemSlots)
        {
            if (slot != null)
            {
                slot.Clear();
            }
        }

        int slotIndex = 0;

        foreach (string itemName in itemOrder)
        {
            if (slotIndex >= itemSlots.Length)
            {
                break;
            }

            InventorySlotUI slot = itemSlots[slotIndex];

            if (slot != null)
            {
                slot.SetItem(itemName, itemCounts[itemName], GetIcon(itemName));
            }

            slotIndex++;
        }
    }

    private string GetStoredItemName(string itemName)
    {
        foreach (string storedItemName in itemOrder)
        {
            if (NamesMatch(storedItemName, itemName))
            {
                return storedItemName;
            }
        }

        return null;
    }

    private Sprite GetIcon(string itemName)
    {
        if (itemIcons == null)
        {
            return null;
        }

        foreach (InventoryItemIcon itemIcon in itemIcons)
        {
            if (itemIcon != null && NamesMatch(itemIcon.itemName, itemName))
            {
                return itemIcon.icon;
            }
        }

        Debug.LogWarning("Missing inventory icon for item: " + itemName);
        return null;
    }

    private bool NamesMatch(string firstName, string secondName)
    {
        return NormalizeItemName(firstName) == NormalizeItemName(secondName);
    }

    private string NormalizeItemName(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return "";
        }

        string normalized = itemName.Trim().Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder();

        foreach (char character in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant()
            .Replace("\u0111", "d");
    }
}


