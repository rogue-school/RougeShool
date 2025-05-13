using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 턴 상태 제어 인터페이스
    /// </summary>
    public interface ITurnStateController
    {
        void RegisterPlayerGuard();

        void ReserveEnemySlot(CombatSlotPosition slot);

        /// <summary>
        /// 다음 상태로 전환을 요청합니다. 실제 전환은 CombatTurnManager에서 처리합니다.
        /// </summary>
        void RequestStateChange(ICombatTurnState nextState);
    }
}
