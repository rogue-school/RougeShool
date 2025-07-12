using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class InventoryItem
    {
        public string id;
        public Sprite icon;
        public int quantity;
    }

    public List<InventorySlot> slots;
    public string potionId = "Potion";

    private Dictionary<string, InventoryItem> inventoryItems = new Dictionary<string, InventoryItem>();

    public void OnItemClicked(string itemId, Sprite itemSprite)
    {
        // 이미 등록된 아이템인지 확인
        foreach (InventorySlot slot in slots)
        {
            if (slot.HasSameItem(itemId))
            {
                slot.AddItem();
                inventoryItems[itemId].quantity++;
                return;
            }
        }

        // 포션이면 0번 슬롯에
        if (itemId == potionId)
        {
            InventorySlot topSlot = slots[0];
            if (topSlot.IsEmpty() || topSlot.HasSameItem(itemId))
            {
                topSlot.SetItem(itemId, itemSprite);
                if (!inventoryItems.ContainsKey(itemId))
                {
                    inventoryItems[itemId] = new InventoryItem { id = itemId, icon = itemSprite, quantity = 1 };
                }
                return;
            }
        }

        // 새 슬롯에 추가
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(itemId, itemSprite);
                inventoryItems[itemId] = new InventoryItem { id = itemId, icon = itemSprite, quantity = 1 };
                return;
            }
        }
    }
}