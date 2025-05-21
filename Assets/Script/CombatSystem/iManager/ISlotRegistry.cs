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
        IEnumerable<ICharacterSlot> GetCharacterSlots();
        IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner);
        IEnumerable<ICombatCardSlot> GetCombatSlots();
        ICharacterSlot GetCharacterSlot(SlotOwner owner);
        ICombatCardSlot GetCombatSlot(CombatSlotPosition position);
        IHandCardSlot GetHandSlot(SkillCardSlotPosition position);

        void Initialize();
    }
}
