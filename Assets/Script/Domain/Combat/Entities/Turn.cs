using System;
using Game.Domain.Combat.ValueObjects;

namespace Game.Domain.Combat.Entities
{
    /// <summary>
    /// 전투 턴 한 회차의 정보를 표현하는 엔티티입니다.
    /// </summary>
    public sealed class Turn
    {
        /// <summary>
        /// 턴 번호입니다. (1부터 시작)
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// 턴의 주체입니다. (플레이어/적)
        /// </summary>
        public TurnType TurnType { get; }

        /// <summary>
        /// 턴이 속한 전투 페이즈입니다.
        /// </summary>
        public CombatPhase Phase { get; }

        /// <summary>
        /// 턴이 이미 처리되었는지 여부입니다.
        /// </summary>
        public bool IsCompleted { get; }

        /// <summary>
        /// 전투 턴을 생성합니다.
        /// </summary>
        /// <param name="number">턴 번호 (1 이상)</param>
        /// <param name="turnType">턴 주체</param>
        /// <param name="phase">전투 페이즈</param>
        /// <param name="isCompleted">완료 여부</param>
        public Turn(int number, TurnType turnType, CombatPhase phase, bool isCompleted = false)
        {
            if (number <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(number),
                    number,
                    "턴 번호는 1 이상이어야 합니다.");
            }

            Number = number;
            TurnType = turnType;
            Phase = phase;
            IsCompleted = isCompleted;
        }

        /// <summary>
        /// 턴을 완료된 상태로 표시한 새 인스턴스를 반환합니다.
        /// </summary>
        public Turn Complete()
        {
            if (IsCompleted)
            {
                return this;
            }

            return new Turn(Number, TurnType, Phase, true);
        }
    }
}


