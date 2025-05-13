using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 턴 상태 제어 인터페이스
    /// </summary>
    public interface ITurnStateController
    {
        void RegisterPlayerGuard();

        void ReserveEnemySlot(CombatSlotPosition slot);
    }
}
