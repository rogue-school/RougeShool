using UnityEngine;
using UnityEngine.UI;
using TMPro;  // 👉 이거 추가!

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;  // 👉 TMP 타입으로 변경

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
        if (countText != null)
        {
            countText.text = count.ToString();
        }
    }
}