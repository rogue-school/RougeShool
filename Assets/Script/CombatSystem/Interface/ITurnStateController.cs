using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ITurnStateController
    {
        void RequestStateChange(ICombatTurnState nextState);
        void ReserveNextEnemySlot(CombatSlotPosition slot);

        void RegisterPlayerGuard();
    }
}
