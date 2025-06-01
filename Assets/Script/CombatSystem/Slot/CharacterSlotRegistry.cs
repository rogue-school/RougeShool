using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using System.Linq;

namespace Game.CombatSystem.Slot
{
    public class CharacterSlotRegistry : MonoBehaviour, ICharacterSlotRegistry
    {
        private List<ICharacterSlot> characterSlots = new();

        public void RegisterCharacterSlots(IEnumerable<ICharacterSlot> slots)
        {
            characterSlots = slots.ToList();
        }

        public IEnumerable<ICharacterSlot> GetAllCharacterSlots()
        {
            return characterSlots;
        }

        public ICharacterSlot GetCharacterSlot(SlotOwner owner)
        {
            return characterSlots.FirstOrDefault(slot => slot.GetOwner() == owner);
        }

        public void Clear()
        {
            characterSlots.Clear();
        }
    }
}
