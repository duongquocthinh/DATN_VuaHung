using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI nameText;

    public void SetItem(string itemName, int amount, Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (quantityText != null)
        {
            quantityText.text = amount > 1 ? amount.ToString() : "";
        }

        if (nameText != null)
        {
            nameText.text = itemName;
        }
    }

    public void Clear()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = "";
        }

        if (nameText != null)
        {
            nameText.text = "";
        }
    }
}
