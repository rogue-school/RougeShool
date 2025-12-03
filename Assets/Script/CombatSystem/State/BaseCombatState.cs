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
        /// 상태 전환 전 완료 검증 (기본 구현)
        /// 파생 클래스에서 override하여 구체적인 검증 로직 구현
        /// </summary>
        public virtual bool CanTransitionToNextState(CombatStateContext context)
        {
            // 기본적으로는 항상 전환 가능
            // 파생 클래스에서 구체적인 검증 로직을 구현해야 함
            LogStateTransition($"[검증] {StateName} 전환 가능 여부 확인");
            return true;
        }

        /// <summary>
        /// 상태 완료 대기 (기본 구현)
        /// 파생 클래스에서 override하여 비동기 작업 완료까지 대기
        /// </summary>
        public virtual System.Collections.IEnumerator WaitForCompletion(CombatStateContext context)
        {
            LogStateTransition($"[대기] {StateName} 완료 대기 중...");
            
            // 기본적으로는 짧은 대기 시간
            yield return new WaitForSeconds(0.1f);
            
            LogStateTransition($"[완료] {StateName} 완료 확인");
        }

        /// <summary>
        /// 다른 상태로 전환 요청 (안전한 전환)
        /// 모든 검증과 완료 대기를 거쳐서 상태 전환
        /// </summary>
        protected void RequestTransitionSafe(CombatStateContext context, ICombatState nextState)
        {
            if (context?.StateMachine != null)
            {
                context.StateMachine.ChangeStateSafe(nextState);
            }
            else
            {
                GameLogger.LogError(
                    $"[{StateName}] 안전한 상태 전환 실패 - StateMachine이 null입니다",
                    GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 다른 상태로 전환 요청 (기존 메서드 - 호환성 유지)
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
        /// 상태 전환 로그 (소환 버그 검증을 위해 항상 출력)
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
