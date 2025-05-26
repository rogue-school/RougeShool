using System.Collections.Generic;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface IHandSlotRegistry
    {
        void RegisterHandSlots(IEnumerable<IHandCardSlot> slots);
        IHandCardSlot GetHandSlot(SkillCardSlotPosition position);
        IEnumerable<IHandCardSlot> GetAllHandSlots();
        IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner);
    }
}
