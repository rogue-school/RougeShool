using System.Collections.Generic;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ISlotRegistry
    {
        // Hand Slots
        void RegisterHandSlots(IHandCardSlot[] slots);
        IHandCardSlot GetHandSlot(SkillCardSlotPosition position);
        IEnumerable<IHandCardSlot> GetAllHandSlots();
        IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner);

        // Combat Slots
        void RegisterCombatSlots(ICombatCardSlot[] slots);
        ICombatCardSlot GetCombatSlot(CombatSlotPosition position);
        ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition);
        IEnumerable<ICombatCardSlot> GetAllCombatSlots();
        IEnumerable<ICombatCardSlot> GetCombatSlots();

        // Character Slots
        void RegisterCharacterSlots(ICharacterSlot[] slots);
        ICharacterSlot GetCharacterSlot(SlotOwner owner);
        IEnumerable<ICharacterSlot> GetCharacterSlots();
    }
}
