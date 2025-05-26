using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface IEnemySlotReservationService
    {
        void ReserveNextEnemySlot(CombatSlotPosition slot);
        CombatSlotPosition? GetReservedEnemySlot();
    }
}
