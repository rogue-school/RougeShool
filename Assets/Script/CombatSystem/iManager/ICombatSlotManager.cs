using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.IManager
{
    /// <summary>
    /// 전투 슬롯(선공/후공 슬롯)을 관리하는 매니저 인터페이스입니다.
    /// 슬롯 UI 접근, 카드 배치, 실행, 클리어 등의 기능을 담당합니다.
    /// </summary>
    public interface ICombatSlotManager
    {
        /// <summary>
        /// 주어진 위치의 전투 슬롯 UI를 반환합니다.
        /// 슬롯 위치는 선공 또는 후공을 나타냅니다.
        /// </summary>
        /// <param name="position">슬롯 위치 (선공 또는 후공)</param>
        /// <returns>전투 카드 슬롯 UI 컴포넌트</returns>
        ICombatCardSlot GetSlot(CombatSlotPosition position);
    }
}
