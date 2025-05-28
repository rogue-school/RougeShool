using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Slot
{
    public class CharacterSlotRegistry : ICharacterSlotRegistry
    {
        private readonly Dictionary<SlotOwner, ICharacterSlot> characterSlots = new();

        public CharacterSlotRegistry(Transform root)
        {
            var slots = root.GetComponentsInChildren<ICharacterSlot>(includeInactive: true);
            RegisterCharacterSlots(slots);
        }

        public void RegisterCharacterSlots(IEnumerable<ICharacterSlot> slots)
        {
            characterSlots.Clear();
            foreach (var slot in slots)
            {
                characterSlots[slot.GetOwner()] = slot;
            }
        }

        public ICharacterSlot GetCharacterSlot(SlotOwner owner)
        {
            characterSlots.TryGetValue(owner, out var slot);
            return slot;
        }

        public IEnumerable<ICharacterSlot> GetAllCharacterSlots() => characterSlots.Values;
    }
}
