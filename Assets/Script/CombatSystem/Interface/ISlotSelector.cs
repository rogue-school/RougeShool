using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 시작 시 슬롯을 배정하는 책임을 집니다.
    /// 적이 먼저 전투 슬롯을 선택하고, 플레이어는 남은 슬롯을 자동으로 할당받습니다.
    /// </summary>
    public interface ISlotSelector
    {
        /// <returns>
        /// 적과 플레이어의 전투 슬롯 위치 쌍을 반환합니다.
        /// 적은 먼저 선택하고, 플레이어는 남은 슬롯을 자동 배정받습니다.
        /// </returns>
        (CombatSlotPosition playerSlot, CombatSlotPosition enemySlot) SelectSlots();
    }
}
