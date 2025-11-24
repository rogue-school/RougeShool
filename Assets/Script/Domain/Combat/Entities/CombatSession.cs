using System;
using System.Collections.Generic;
using Game.Domain.Combat.ValueObjects;

namespace Game.Domain.Combat.Entities
{
    /// <summary>
    /// 하나의 전투 세션을 표현하는 엔티티입니다.
    /// 턴 기록과 현재 슬롯 상태, 전투 페이즈를 관리합니다.
    /// </summary>
    public sealed class CombatSession
    {
        private readonly List<Turn> _turns;
        private readonly Dictionary<SlotPosition, CombatSlot> _slots;

        /// <summary>
        /// 현재 전투 페이즈입니다.
        /// </summary>
        public CombatPhase Phase { get; private set; }

        /// <summary>
        /// 현재 턴 번호입니다. (턴이 없으면 0)
        /// </summary>
        public int CurrentTurnNumber => _turns.Count == 0 ? 0 : _turns[^1].Number;

        /// <summary>
        /// 현재 턴입니다. (없으면 null)
        /// </summary>
        public Turn CurrentTurn => _turns.Count == 0 ? null : _turns[^1];

        /// <summary>
        /// 생성자.
        /// </summary>
        /// <param name="initialPhase">초기 전투 페이즈</param>
        /// <param name="initialSlots">초기 슬롯 상태</param>
        public CombatSession(
            CombatPhase initialPhase,
            IEnumerable<CombatSlot> initialSlots)
        {
            Phase = initialPhase;
            _turns = new List<Turn>();
            _slots = new Dictionary<SlotPosition, CombatSlot>();

            if (initialSlots != null)
            {
                foreach (var slot in initialSlots)
                {
                    _slots[slot.Position] = slot;
                }
            }
        }

        /// <summary>
        /// 새로운 턴을 시작합니다.
        /// </summary>
        /// <param name="turnType">턴 주체</param>
        public Turn StartNextTurn(TurnType turnType)
        {
            int nextNumber = CurrentTurnNumber + 1;
            var turn = new Turn(nextNumber, turnType, Phase);
            _turns.Add(turn);
            return turn;
        }

        /// <summary>
        /// 전투 페이즈를 변경합니다.
        /// </summary>
        /// <param name="nextPhase">다음 페이즈</param>
        public void ChangePhase(CombatPhase nextPhase)
        {
            Phase = nextPhase;
        }

        /// <summary>
        /// 특정 슬롯의 현재 상태를 반환합니다.
        /// </summary>
        /// <param name="position">조회할 슬롯 위치</param>
        public CombatSlot GetSlot(SlotPosition position)
        {
            return _slots.TryGetValue(position, out var slot)
                ? slot
                : new CombatSlot(position);
        }

        /// <summary>
        /// 슬롯 상태를 변경합니다.
        /// </summary>
        /// <param name="slot">변경할 슬롯</param>
        public void SetSlot(CombatSlot slot)
        {
            if (slot == null)
            {
                throw new ArgumentNullException(nameof(slot));
            }

            _slots[slot.Position] = slot;
        }
    }
}


