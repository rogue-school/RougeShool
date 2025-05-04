using System;
using Game.Interface;

namespace Game.Events
{
    /// <summary>
    /// 카드가 슬롯에 드롭되었을 때 발생하는 글로벌 이벤트 시스템입니다.
    /// </summary>
    public static class CardDropEventSystem
    {
        public static event Action<ISkillCard, ICardSlot> OnCardDropped;

        public static void NotifyCardDropped(ISkillCard card, ICardSlot slot)
        {
            OnCardDropped?.Invoke(card, slot);
        }
    }
}
