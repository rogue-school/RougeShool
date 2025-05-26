using System.Collections.Generic;
using System.Linq;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Slot
{
    public class HandSlotRegistry : IHandSlotRegistry
    {
        private readonly Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();

        public void RegisterHandSlots(IEnumerable<IHandCardSlot> slots)
        {
            handSlots.Clear();
            foreach (var slot in slots)
            {
                handSlots[slot.GetSlotPosition()] = slot;
            }
        }

        public IHandCardSlot GetHandSlot(SkillCardSlotPosition position)
        {
            handSlots.TryGetValue(position, out var slot);
            return slot;
        }

        public IEnumerable<IHandCardSlot> GetAllHandSlots() => handSlots.Values;

        public IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner)
        {
            return handSlots.Values.Where(s => s.GetOwner() == owner);
        }
    }
}
