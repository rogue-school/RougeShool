using System.Collections.Generic;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.IManager
{
    /// <summary>
    /// 전투 슬롯을 통합적으로 관리하는 슬롯 레지스트리 인터페이스입니다.
    /// </summary>
    public interface ISlotRegistry
    {
        /// <summary>
        /// 모든 캐릭터 슬롯을 반환합니다.
        /// </summary>
        IEnumerable<ICharacterSlot> GetCharacterSlots();

        /// <summary>
        /// 지정한 소유자의 핸드 슬롯을 반환합니다.
        /// </summary>
        IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner);

        /// <summary>
        /// 모든 전투 실행 슬롯을 반환합니다.
        /// </summary>
        IEnumerable<ICombatCardSlot> GetCombatSlots();

        /// <summary>
        /// 지정한 소유자의 캐릭터 슬롯을 단일 반환합니다.
        /// </summary>
        ICharacterSlot GetCharacterSlot(SlotOwner owner);
    }
}
