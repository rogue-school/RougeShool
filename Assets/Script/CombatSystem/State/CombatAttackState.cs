using UnityEngine;
using Game.CombatSystem.Manager;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 공격 상태 (현 턴 시스템 준수)
    /// - 턴 진행/실행은 TurnManager/ExecutionManager가 담당
    /// - 상태는 로그/연출 훅만 담당 (중복 전진 방지)
    /// </summary>
    public class CombatAttackState
    {
        private readonly TurnManager turnManager;
        private readonly CombatExecutionManager executionManager;

        public CombatAttackState(
            TurnManager turnManager,
            CombatExecutionManager executionManager
        )
        {
            this.turnManager = turnManager;
            this.executionManager = executionManager;
        }

        public void EnterState()
        {
            GameLogger.LogInfo("[STATE] Attack 진입", GameLogger.LogCategory.Combat);
            // 이 상태에서 별도 실행/턴 전진을 수행하지 않습니다.
            // TurnManager가 슬롯 전진/생성/자동 실행을 처리합니다.
        }

        /// <summary>
        /// 상태 실행 중 특별한 작업 없음
        /// </summary>
        public void ExecuteState() { }

        public void ExitState()
        {
            GameLogger.LogInfo("[STATE] Attack 종료", GameLogger.LogCategory.Combat);
        }
    }
}
