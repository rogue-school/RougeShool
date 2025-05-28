using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Slot
{
    public class CombatSlotRegistry : ICombatSlotRegistry
    {
        private readonly Dictionary<CombatSlotPosition, ICombatCardSlot> combatSlots = new();

        public CombatSlotRegistry(Transform root)
        {
            var slots = root.GetComponentsInChildren<ICombatCardSlot>(includeInactive: true);
            RegisterCombatSlots(slots);
        }

        public void RegisterCombatSlots(IEnumerable<ICombatCardSlot> slots)
        {
            combatSlots.Clear();
            foreach (var slot in slots)
            {
                var position = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
                combatSlots[position] = slot;
            }
        }

        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            combatSlots.TryGetValue(position, out var slot);
            return slot;
        }

        public ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition)
        {
            var execSlot = SlotPositionUtil.ToExecutionSlot(fieldPosition);
            return GetCombatSlot(execSlot);
        }

        public IEnumerable<ICombatCardSlot> GetAllCombatSlots() => combatSlots.Values;
    }
}
