using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Text countText;

    private string itemId;
    private int count;

    public void SetItem(string id, Sprite sprite)
    {
        itemId = id;
        icon.sprite = sprite;
        icon.enabled = true;
        count = 1;
        UpdateUI();
    }

    public void AddItem()
    {
        count++;
        UpdateUI();
    }

    public bool HasSameItem(string id)
    {
        return itemId == id;
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(itemId);
    }

    private void UpdateUI()
    {
        countText.text = count.ToString();
    }
}