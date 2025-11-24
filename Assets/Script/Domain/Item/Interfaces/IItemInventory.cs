using System.Collections.Generic;
using Game.Domain.Item.ValueObjects;

namespace Game.Domain.Item.Interfaces
{
    /// <summary>
    /// 도메인 레벨에서의 아이템 인벤토리 인터페이스입니다.
    /// </summary>
    public interface IItemInventory
    {
        /// <summary>
        /// 모든 액티브 아이템 슬롯을 반환합니다.
        /// </summary>
        IReadOnlyList<ActiveItemSlot> GetActiveSlots();

        /// <summary>
        /// 액티브 인벤토리가 가득 찼는지 여부를 반환합니다.
        /// </summary>
        bool IsActiveInventoryFull();
    }
}


