using System;
using System.Collections.Generic;
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

    public void AddItem(string itemName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return;
        }

        if (!itemCounts.ContainsKey(itemName))
        {
            itemCounts[itemName] = 0;
        }

        itemCounts[itemName] += amount;
        RefreshInventoryUI();
        Debug.Log(itemName + " x" + itemCounts[itemName]);
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

        foreach (KeyValuePair<string, int> item in itemCounts)
        {
            if (slotIndex >= itemSlots.Length)
            {
                break;
            }

            InventorySlotUI slot = itemSlots[slotIndex];

            if (slot != null)
            {
                slot.SetItem(item.Key, item.Value, GetIcon(item.Key));
            }

            slotIndex++;
        }
    }

    private Sprite GetIcon(string itemName)
    {
        if (itemIcons == null)
        {
            return null;
        }

        foreach (InventoryItemIcon itemIcon in itemIcons)
        {
            if (itemIcon != null && itemIcon.itemName == itemName)
            {
                return itemIcon.icon;
            }
        }

        return null;
    }
}
