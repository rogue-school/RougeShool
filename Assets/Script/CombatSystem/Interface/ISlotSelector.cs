using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ISlotSelector
    {
        (CombatSlotPosition playerSlot, CombatSlotPosition enemySlot) SelectSlots();
    }
}
