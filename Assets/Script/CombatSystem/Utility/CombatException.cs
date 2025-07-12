using System;

namespace Game.CombatSystem.Utility
{
    /// <summary>
    /// 전투 시스템에서 발생하는 예외를 처리하는 기본 예외 클래스
    /// </summary>
    public class CombatException : Exception
    {
        public CombatException() : base() { }
        public CombatException(string message) : base(message) { }
        public CombatException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 슬롯 관련 예외
    /// </summary>
    public class CombatSlotException : CombatException
    {
        public CombatSlotException(string message) : base(message) { }
        public CombatSlotException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 카드 관련 예외
    /// </summary>
    public class CombatCardException : CombatException
    {
        public CombatCardException(string message) : base(message) { }
        public CombatCardException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 상태 관련 예외
    /// </summary>
    public class CombatStateException : CombatException
    {
        public CombatStateException(string message) : base(message) { }
        public CombatStateException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 애니메이션 관련 예외
    /// </summary>
    public class CombatAnimationException : CombatException
    {
        public CombatAnimationException(string message) : base(message) { }
        public CombatAnimationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 초기화 관련 예외
    /// </summary>
    public class CombatInitializationException : CombatException
    {
        public CombatInitializationException(string message) : base(message) { }
        public CombatInitializationException(string message, Exception innerException) : base(message, innerException) { }
    }
} 