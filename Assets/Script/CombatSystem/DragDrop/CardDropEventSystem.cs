using System;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.DragDrop
{
    /// <summary>
    /// 카드가 전투 슬롯에 드롭되었을 때 발생하는 이벤트 시스템입니다.
    /// 카드 UI → 슬롯 간 드롭 성공 시 전역적으로 알림을 제공합니다.
    /// </summary>
    public static class CardDropEventSystem
    {
        /// <summary>
        /// 카드가 전투 슬롯에 성공적으로 드롭되었을 때 발생하는 이벤트입니다.
        /// 첫 번째 인자는 드롭된 카드, 두 번째 인자는 드롭 대상 슬롯입니다.
        /// </summary>
        public static event Action<ISkillCard, ICombatCardSlot> OnCombatCardDropped;

        /// <summary>
        /// 카드가 전투 슬롯에 드롭되었음을 이벤트로 알립니다.
        /// 드래그 앤 드롭 성공 후 호출되어야 합니다.
        /// </summary>
        /// <param name="card">드롭된 카드</param>
        /// <param name="slot">드롭된 슬롯</param>
        public static void NotifyCombatCardDropped(ISkillCard card, ICombatCardSlot slot)
        {
            OnCombatCardDropped?.Invoke(card, slot);
        }
    }
}
