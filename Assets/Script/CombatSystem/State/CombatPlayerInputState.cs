using UnityEngine;
using Game.CombatSystem.Manager;
using Game.CoreSystem.Utility;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 플레이어 입력 대기 상태 (현 턴 시스템 준수)
    /// - 드래그/드롭 등 입력 대기만 담당하고, 턴 전환/실행은 TurnManager/ExecutionManager가 처리합니다.
    /// </summary>
    public class CombatPlayerInputState
    {
        private readonly TurnManager turnManager;

        public CombatPlayerInputState(TurnManager turnManager)
        {
            this.turnManager = turnManager;
        }

        public void EnterState()
        {
            GameLogger.LogInfo("[STATE] PlayerInput 진입 - 플레이어 입력 대기", GameLogger.LogCategory.Combat);
            // 현재 구조에서는 핸드 입력 활성화가 기본값이므로 추가 조치 불필요
        }

        public void ExecuteState()
        {
            // 필요 시 UI 힌트 갱신 등 추가
        }

        public void ExitState()
        {
            GameLogger.LogInfo("[STATE] PlayerInput 종료", GameLogger.LogCategory.Combat);
        }
    }
}
