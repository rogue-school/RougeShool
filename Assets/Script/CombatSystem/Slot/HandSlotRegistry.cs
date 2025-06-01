using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Slot;

namespace Game.CombatSystem.Slot
{
    public class HandSlotRegistry : MonoBehaviour, IHandSlotRegistry
    {
        private List<IHandCardSlot> handSlots = new();

        public void RegisterHandSlots(IEnumerable<IHandCardSlot> slots)
        {
            handSlots = slots.ToList();
        }

        public IHandCardSlot GetHandSlot(SkillCardSlotPosition position)
        {
            return handSlots.FirstOrDefault(slot => slot.GetSlotPosition() == position);
        }

        public IEnumerable<IHandCardSlot> GetAllHandSlots()
        {
            return handSlots;
        }

        public IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner)
        {
            return handSlots.Where(slot => slot.GetOwner() == owner);
        }
    }
}
