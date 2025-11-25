using Game.Domain.Combat.ValueObjects;

namespace Game.Application.Battle
{
    /// <summary>
    /// 전투가 시작되었을 때 발행되는 이벤트입니다.
    /// </summary>
    public sealed class CombatStartedEvent
    {
        /// <summary>첫 턴 번호입니다.</summary>
        public int FirstTurnNumber { get; }

        /// <summary>첫 턴 주체입니다.</summary>
        public TurnType FirstTurnType { get; }

        public CombatStartedEvent(int firstTurnNumber, TurnType firstTurnType)
        {
            FirstTurnNumber = firstTurnNumber;
            FirstTurnType = firstTurnType;
        }
    }

    /// <summary>
    /// 턴이 변경되었을 때 발행되는 이벤트입니다.
    /// </summary>
    public sealed class TurnChangedEvent
    {
        /// <summary>현재 턴 번호입니다.</summary>
        public int TurnNumber { get; }

        /// <summary>현재 턴 주체입니다.</summary>
        public TurnType TurnType { get; }

        /// <summary>현재 전투 페이즈입니다.</summary>
        public CombatPhase Phase { get; }

        public TurnChangedEvent(int turnNumber, TurnType turnType, CombatPhase phase)
        {
            TurnNumber = turnNumber;
            TurnType = turnType;
            Phase = phase;
        }
    }
}


