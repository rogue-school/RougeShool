using UnityEngine;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 기본 컴뱃 턴 처리 상태입니다.
    /// 현재 특별한 동작은 없으며 상태 전환 예시를 포함합니다.
    /// </summary>
    public class DefaultCombatState : ICombatTurnState
    {
        private readonly CombatTurnManager turnManager;

        public DefaultCombatState(CombatTurnManager manager)
        {
            turnManager = manager;
        }

        public void EnterState()
        {
            Debug.Log("[DefaultCombatState] 상태 진입");
        }

        public void ExecuteState()
        {
            // 예시 로직: 상태 유지 또는 전환 가능
            Debug.Log("[DefaultCombatState] 기본 턴 상태 실행 중");

            // 예: 상태 전환
            // turnManager.SetState(new PlayerGuardedState(turnManager));
        }

        public void ExitState()
        {
            Debug.Log("[DefaultCombatState] 상태 종료");
        }
    }
}
