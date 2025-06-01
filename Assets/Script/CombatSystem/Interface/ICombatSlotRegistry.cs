using System.Collections.Generic;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Interface
{
    public interface ICombatSlotRegistry
    {
        void RegisterCombatSlots(IEnumerable<ICombatCardSlot> slots);
        ICombatCardSlot GetCombatSlot(CombatSlotPosition position);
        ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition);
        IEnumerable<ICombatCardSlot> GetAllCombatSlots();
    }
}
