using System.Collections.Generic;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 카드 슬롯들을 등록하고 조회할 수 있는 슬롯 레지스트리 인터페이스입니다.
    /// 슬롯 포지션(Position 또는 FieldPosition)에 따라 슬롯 정보를 제공합니다.
    /// </summary>
    public interface ICombatSlotRegistry
    {
        /// <summary>
        /// 전투 카드 슬롯들을 등록합니다.
        /// 일반적으로 전투 시작 시 슬롯을 한 번 등록합니다.
        /// </summary>
        /// <param name="slots">등록할 슬롯 목록</param>
        void RegisterCombatSlots(IEnumerable<ICombatCardSlot> slots);

        /// <summary>
        /// CombatSlotPosition 기반으로 슬롯을 조회합니다.
        /// 예: 선공 슬롯, 후공 슬롯 등 단일 역할 기반 슬롯
        /// </summary>
        /// <param name="position">조회할 슬롯 포지션</param>
        /// <returns>슬롯 객체</returns>
        ICombatCardSlot GetCombatSlot(CombatSlotPosition position);

        /// <summary>
        /// CombatFieldSlotPosition 기반으로 슬롯을 조회합니다.
        /// 예: 플레이어/적 + 선공/후공 등의 복합 위치
        /// </summary>
        /// <param name="fieldPosition">조회할 필드 슬롯 포지션</param>
        /// <returns>슬롯 객체</returns>
        ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition);

        /// <summary>
        /// 등록된 모든 슬롯들을 반환합니다.
        /// </summary>
        /// <returns>슬롯 목록</returns>
        IEnumerable<ICombatCardSlot> GetAllCombatSlots();
    }
}
