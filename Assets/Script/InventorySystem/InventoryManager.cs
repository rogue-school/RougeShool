using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] slots;

    public void OnItemClicked(string itemId, Sprite itemSprite)
    {
        // 포션은 항상 Slot1에 들어가게
        if (itemId == "potion")
        {
            slots[0].AddItem(itemId, itemSprite);
            return;
        }

        // 다른 아이템은 빈 슬롯에 들어가게
        foreach (var slot in slots)
        {
            if (!slot.HasSameItem(itemId))
                continue;

            slot.AddItem(itemId, itemSprite);
            return;
        }

        foreach (var slot in slots)
        {
            if (slot.icon.sprite == null) // 빈 슬롯
            {
                slot.AddItem(itemId, itemSprite);
                return;
            }
        }
    }
}