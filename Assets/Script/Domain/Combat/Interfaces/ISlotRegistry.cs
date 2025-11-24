using Game.Domain.Combat.Entities;
using Game.Domain.Combat.ValueObjects;

namespace Game.Domain.Combat.Interfaces
{
    /// <summary>
    /// 전투 슬롯 상태를 관리하는 도메인 인터페이스입니다.
    /// </summary>
    public interface ISlotRegistry
    {
        /// <summary>
        /// 특정 위치의 슬롯 상태를 반환합니다.
        /// </summary>
        /// <param name="position">조회할 슬롯 위치</param>
        CombatSlot GetSlot(SlotPosition position);

        /// <summary>
        /// 슬롯 상태를 저장합니다.
        /// </summary>
        /// <param name="slot">저장할 슬롯</param>
        void SetSlot(CombatSlot slot);

        /// <summary>
        /// 모든 슬롯을 초기 상태로 되돌립니다.
        /// </summary>
        void ClearAllSlots();
    }
}


