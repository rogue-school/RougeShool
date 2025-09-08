using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 적의 다음 공격 슬롯을 예약하고 조회할 수 있는 서비스 인터페이스입니다.
    /// 예를 들어 적이 다음 턴에 어느 슬롯에 카드를 낼 것인지 미리 결정할 때 사용됩니다.
    /// </summary>
    public interface IEnemySlotReservationService
    {
        /// <summary>
        /// 적의 다음 공격을 위한 슬롯을 예약합니다.
        /// </summary>
        /// <param name="slot">예약할 슬롯 위치</param>
        void ReserveNextEnemySlot(CombatSlotPosition slot);

        /// <summary>
        /// 예약된 적의 슬롯을 반환합니다.
        /// 슬롯이 예약되지 않은 경우 null을 반환합니다.
        /// </summary>
        /// <returns>예약된 슬롯 위치 또는 null</returns>
        CombatSlotPosition? GetReservedEnemySlot();
    }
}
