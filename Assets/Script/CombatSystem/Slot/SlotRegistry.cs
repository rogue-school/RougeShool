using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Slot
{
    public class SlotRegistry : MonoBehaviour, ISlotRegistry
    {
        [SerializeField] private HandSlotRegistry handSlotRegistry;
        [SerializeField] private CombatSlotRegistry combatSlotRegistry;
        [SerializeField] private CharacterSlotRegistry characterSlotRegistry;

        public bool IsInitialized { get; private set; }

        public IHandSlotRegistry GetHandSlotRegistry() => handSlotRegistry;
        public ICombatSlotRegistry GetCombatSlotRegistry() => combatSlotRegistry;
        public ICharacterSlotRegistry GetCharacterSlotRegistry() => characterSlotRegistry;

        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            return combatSlotRegistry?.GetSlotByPosition(position);
        }

        public ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition position)
        {
            return combatSlotRegistry.GetCombatSlot(position);
        }

        public void MarkInitialized() => IsInitialized = true;
    }
}
