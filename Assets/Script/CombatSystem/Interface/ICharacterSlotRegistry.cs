using System.Collections.Generic;
using Game.CombatSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 캐릭터 슬롯 정보를 등록하고 조회하는 전투 시스템의 슬롯 레지스트리 인터페이스입니다.
    /// 플레이어/적의 슬롯 정보를 중앙에서 관리합니다.
    /// </summary>
    public interface ICharacterSlotRegistry
    {
        /// <summary>
        /// 슬롯 리스트를 한 번에 등록합니다.
        /// 전투 시작 시 한 번만 호출되는 것이 일반적입니다.
        /// </summary>
        /// <param name="slots">등록할 캐릭터 슬롯 목록</param>
        void RegisterCharacterSlots(IEnumerable<ICharacterSlot> slots);

        /// <summary>
        /// 소유자(SlotOwner)에 해당하는 캐릭터 슬롯을 반환합니다.
        /// </summary>
        /// <param name="owner">플레이어 또는 적 소유자</param>
        /// <returns>해당 소유자의 캐릭터 슬롯</returns>
        ICharacterSlot GetCharacterSlot(SlotOwner owner);

        /// <summary>
        /// 등록된 모든 캐릭터 슬롯을 반환합니다.
        /// </summary>
        /// <returns>전체 슬롯 컬렉션</returns>
        IEnumerable<ICharacterSlot> GetAllCharacterSlots();
    }
}
