using System.Collections.Generic;
using Game.CombatSystem.Utility;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ICharacterSlotRegistry
    {
        void RegisterCharacterSlots(IEnumerable<ICharacterSlot> slots);
        ICharacterSlot GetCharacterSlot(SlotOwner owner);
        IEnumerable<ICharacterSlot> GetAllCharacterSlots();
    }
}
