using Game.CombatSystem.Slot;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ITurnStateController
    {
        /// <summary>
        /// 다음 전투 상태로 전이 요청
        /// </summary>
        void RequestStateChange(ICombatTurnState nextState);

        /// <summary>
        /// 적이 사용할 슬롯을 예약
        /// </summary>
        void ReserveNextEnemySlot(CombatSlotPosition slot);

        /// <summary>
        /// 예약된 적 슬롯 위치 반환
        /// </summary>
        CombatSlotPosition? GetReservedEnemySlot();
    }
}
