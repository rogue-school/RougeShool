using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 시작 시 적과 플레이어의 전투 슬롯을 배정하는 인터페이스입니다.
    /// 적은 먼저 슬롯을 선택하고, 플레이어는 남은 슬롯을 자동으로 할당받습니다.
    /// </summary>
    public interface ISlotSelector
    {
        /// <summary>
        /// 전투 슬롯을 배정합니다.
        /// 적은 우선적으로 슬롯을 선택하고, 플레이어는 남은 슬롯을 자동으로 할당받습니다.
        /// </summary>
        /// <returns>
        /// 플레이어와 적의 전투 슬롯 위치를 쌍으로 반환합니다.
        /// (playerSlot: 플레이어 슬롯 위치, enemySlot: 적 슬롯 위치)
        /// </returns>
        (CombatSlotPosition playerSlot, CombatSlotPosition enemySlot) SelectSlots();
    }
}
