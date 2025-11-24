using System;
using Game.Domain.Combat.Entities;
using Game.Domain.Combat.Interfaces;
using Game.Domain.Combat.ValueObjects;

namespace Game.Application.Battle
{
    /// <summary>
    /// 전투 슬롯 간에 카드를 이동시키는 애플리케이션 유스케이스입니다.
    /// 도메인 슬롯 레지스트리를 사용하여 슬롯의 카드 상태만 조정하며,
    /// UI 애니메이션이나 이펙트는 포함하지 않습니다.
    /// </summary>
    public sealed class MoveSlotUseCase
    {
        private readonly ISlotRegistry _slotRegistry;

        /// <summary>
        /// 슬롯 이동 유스케이스를 생성합니다.
        /// </summary>
        /// <param name="slotRegistry">전투 슬롯 상태를 관리하는 도메인 레지스트리</param>
        /// <exception cref="ArgumentNullException">slotRegistry가 null인 경우</exception>
        public MoveSlotUseCase(ISlotRegistry slotRegistry)
        {
            _slotRegistry = slotRegistry ?? throw new ArgumentNullException(nameof(slotRegistry), "슬롯 레지스트리는 null일 수 없습니다.");
        }

        /// <summary>
        /// 한 슬롯에 있는 카드를 다른 슬롯으로 이동시킵니다.
        /// UI 이동이나 애니메이션은 포함하지 않고, 도메인 데이터만 변경합니다.
        /// </summary>
        /// <param name="from">이동할 카드가 있는 슬롯 위치</param>
        /// <param name="to">카드를 이동시킬 대상 슬롯 위치</param>
        /// <exception cref="InvalidOperationException">
        /// - 같은 슬롯으로 이동을 시도한 경우
        /// - 출발 슬롯이 존재하지 않거나 비어 있는 경우
        /// </exception>
        public void Execute(SlotPosition from, SlotPosition to)
        {
            if (from == to)
            {
                throw new InvalidOperationException("같은 슬롯으로 카드를 이동시킬 수 없습니다.");
            }

            CombatSlot fromSlot = _slotRegistry.GetSlot(from);
            if (fromSlot == null || fromSlot.IsEmpty)
            {
                throw new InvalidOperationException("출발 슬롯에 이동할 카드가 없습니다.");
            }

            CombatSlot toSlot = _slotRegistry.GetSlot(to);
            if (toSlot == null)
            {
                toSlot = new CombatSlot(to);
            }

            var card = fromSlot.Card;
            fromSlot.ClearCard();
            toSlot.AssignCard(card);

            _slotRegistry.SetSlot(fromSlot);
            _slotRegistry.SetSlot(toSlot);
        }
    }
}


