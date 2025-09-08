using System.Collections.Generic;
using Game.CombatSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 캐릭터 슬롯 정보를 중앙에서 등록 및 관리하는 인터페이스입니다.
    /// 플레이어 및 적 캐릭터가 위치하는 슬롯을 전투 시스템에서 통합적으로 조회할 수 있습니다.
    /// </summary>
    public interface ICharacterSlotRegistry
    {
        /// <summary>
        /// 캐릭터 슬롯들을 한 번에 등록합니다.
        /// 일반적으로 전투 초기화 시 한 번 호출됩니다.
        /// </summary>
        /// <param name="slots">등록할 캐릭터 슬롯 목록</param>
        void RegisterCharacterSlots(IEnumerable<ICharacterSlot> slots);

        /// <summary>
        /// 지정된 소유자(SlotOwner)에 해당하는 캐릭터 슬롯을 반환합니다.
        /// 예: <see cref="SlotOwner.PLAYER"/> 또는 <see cref="SlotOwner.ENEMY"/>
        /// </summary>
        /// <param name="owner">슬롯 소유자 (플레이어 또는 적)</param>
        /// <returns>해당 소유자에 연결된 캐릭터 슬롯</returns>
        ICharacterSlot GetCharacterSlot(SlotOwner owner);

        /// <summary>
        /// 등록된 모든 캐릭터 슬롯들을 반환합니다.
        /// 주로 디버깅이나 시스템 전체 상태 점검에 활용됩니다.
        /// </summary>
        /// <returns>전체 캐릭터 슬롯 컬렉션</returns>
        IEnumerable<ICharacterSlot> GetAllCharacterSlots();
    }
}
