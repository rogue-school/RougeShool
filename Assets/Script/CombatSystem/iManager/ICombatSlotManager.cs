using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.IManager
{
    /// <summary>
    /// 전투 실행 슬롯(선공/후공)을 관리하는 인터페이스입니다.
    /// 슬롯 접근, 카드 실행, 클리어 등을 담당합니다.
    /// </summary>
    public interface ICombatSlotManager
    {
        /// <summary>
        /// 해당 슬롯 위치의 슬롯 UI를 반환합니다.
        /// </summary>
        ICombatCardSlot GetSlot(CombatSlotPosition position);
    }
}