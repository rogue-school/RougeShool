using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;                 // 아이템 아이콘
    public TextMeshProUGUI countText;  // 수량 표시
    public TextMeshProUGUI nameText;   // 아이템 이름 표시 (새로 추가)

    private string itemId;
    private int count;

    void Start()
    {
        if (icon != null)
            icon.enabled = false;

        if (countText != null)
            countText.text = "";

        if (nameText != null)
            nameText.text = "";  // 시작 시 이름 숨김
    }

    public void AddItem(string id, Sprite sprite, int amount = 1, string displayName = "")
    {
        itemId = id;
        count += amount;

        icon.sprite = sprite;
        icon.enabled = true;

        countText.text = count.ToString();

        if (!string.IsNullOrEmpty(displayName))
            nameText.text = displayName;
    }

    public bool HasSameItem(string id)
    {
        return itemId == id;
    }
}