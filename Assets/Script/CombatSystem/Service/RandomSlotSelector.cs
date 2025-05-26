using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;

namespace Game.CombatSystem.Service
{
    public class RandomSlotSelector : ISlotSelector
    {
        public (CombatSlotPosition playerSlot, CombatSlotPosition enemySlot) SelectSlots()
        {
            bool flip = Random.value < 0.5f;
            return flip
                ? (CombatSlotPosition.FIRST, CombatSlotPosition.SECOND)
                : (CombatSlotPosition.SECOND, CombatSlotPosition.FIRST);
        }
    }
}
