using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Slot
{
    public class HandSlotRegistry : IHandSlotRegistry
    {
        private readonly Dictionary<SkillCardSlotPosition, IHandCardSlot> slotMap = new();
        private readonly List<IHandCardSlot> allSlots = new();

        public HandSlotRegistry(Transform root)
        {
            allSlots.Clear();
            slotMap.Clear();

            var slots = root.GetComponentsInChildren<IHandCardSlot>(true);
            foreach (var slot in slots)
            {
                var pos = slot.GetSlotPosition();
                slotMap[pos] = slot;
                allSlots.Add(slot);
            }
        }

        public void RegisterHandSlots(IEnumerable<IHandCardSlot> slots)
        {
            slotMap.Clear();
            allSlots.Clear();

            foreach (var slot in slots)
            {
                slotMap[slot.GetSlotPosition()] = slot;
                allSlots.Add(slot);
            }
        }

        public IHandCardSlot GetHandSlot(SkillCardSlotPosition position)
        {
            return slotMap.TryGetValue(position, out var slot) ? slot : null;
        }

        public IEnumerable<IHandCardSlot> GetAllHandSlots()
        {
            return allSlots;
        }

        public IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner)
        {
            return allSlots.Where(slot => slot.GetOwner() == owner);
        }
    }
}
