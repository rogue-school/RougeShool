using System;
using Game.Interface;

namespace Game.Events
{
    /// <summary>
    /// 카드가 전투 슬롯에 드롭되었을 때 발생하는 이벤트 시스템입니다.
    /// </summary>
    public static class CardDropEventSystem
    {
        /// <summary>
        /// 카드가 전투 슬롯에 드롭되었을 때 발생
        /// </summary>
        public static event Action<ISkillCard, ICombatCardSlot> OnCombatCardDropped;

        public static void NotifyCombatCardDropped(ISkillCard card, ICombatCardSlot slot)
        {
            OnCombatCardDropped?.Invoke(card, slot);
        }
    }
}

