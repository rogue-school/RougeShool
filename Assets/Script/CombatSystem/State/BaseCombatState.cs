using UnityEngine;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 상태의 기본 추상 클래스
    /// 공통 기능과 기본 동작을 제공하며, 구체적인 상태는 이를 상속받아 구현합니다.
    /// </summary>
    public abstract class BaseCombatState : ICombatState
    {
        public abstract string StateName { get; }

        // 기본값 설정 (파생 클래스에서 override 가능)
        public virtual bool AllowPlayerCardDrag => false;
        public virtual bool AllowEnemyAutoExecution => false;
        public virtual bool AllowSlotMovement => false;
        public virtual bool AllowTurnSwitch => false;

        /// <summary>
        /// 상태 진입
        /// </summary>
        public virtual void OnEnter(CombatStateContext context)
        {
            LogStateTransition($"진입: {StateName}");
        }

        /// <summary>
        /// 상태 업데이트 (필요시 override)
        /// </summary>
        public virtual void OnUpdate(CombatStateContext context)
        {
            // 기본적으로 아무것도 하지 않음
        }

        /// <summary>
        /// 상태 종료
        /// </summary>
        public virtual void OnExit(CombatStateContext context)
        {
            LogStateTransition($"종료: {StateName}");
        }

        /// <summary>
        /// 다른 상태로 전환 요청
        /// </summary>
        protected void RequestTransition(CombatStateContext context, ICombatState nextState)
        {
            if (context?.StateMachine != null)
            {
                context.StateMachine.ChangeState(nextState);
            }
            else
            {
                GameLogger.LogError(
                    $"[{StateName}] 상태 전환 실패 - StateMachine이 null입니다",
                    GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 상태 전환 로그
        /// </summary>
        protected void LogStateTransition(string message)
        {
            GameLogger.LogInfo(
                $"[CombatState] {message}",
                GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 경고 로그
        /// </summary>
        protected void LogWarning(string message)
        {
            GameLogger.LogWarning(
                $"[{StateName}] {message}",
                GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 에러 로그
        /// </summary>
        protected void LogError(string message)
        {
            GameLogger.LogError(
                $"[{StateName}] {message}",
                GameLogger.LogCategory.Error);
        }
    }
}
