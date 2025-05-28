using UnityEngine;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Slot
{
    public class SlotRegistry : MonoBehaviour, ISlotRegistry
    {
        [SerializeField] private Transform handSlotRoot;
        [SerializeField] private Transform combatSlotRoot;
        [SerializeField] private Transform characterSlotRoot;

        private IHandSlotRegistry handSlots;
        private ICombatSlotRegistry combatSlots;
        private ICharacterSlotRegistry characterSlots;

        public void Initialize()
        {
            handSlots = new HandSlotRegistry(handSlotRoot);
            combatSlots = new CombatSlotRegistry(combatSlotRoot);
            characterSlots = new CharacterSlotRegistry(characterSlotRoot);
        }

        public IHandSlotRegistry GetHandSlotRegistry() => handSlots;
        public ICombatSlotRegistry GetCombatSlotRegistry() => combatSlots;
        public ICharacterSlotRegistry GetCharacterSlotRegistry() => characterSlots;

        // ✅ ISlotRegistry 인터페이스 구현
        public ICombatCardSlot GetCombatSlot(CombatSlotPosition position)
        {
            return combatSlots.GetCombatSlot(position);
        }
    }
}
