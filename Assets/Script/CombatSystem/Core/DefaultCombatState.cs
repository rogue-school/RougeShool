using UnityEngine;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// [DefaultCombatState]
    /// 기본 턴 상태이며, 디버깅 또는 초기 상태로 사용됩니다.
    /// 이 상태에서는 아무 로직도 실행되지 않으며,
    /// 상태 전이 흐름을 점검하는 데 사용됩니다.
    /// </summary>
    public class DefaultCombatState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;

        public DefaultCombatState(ICombatTurnManager manager)
        {
            turnManager = manager;
        }

        public void EnterState()
        {
            Debug.Log("[DefaultCombatState] 상태 진입");
        }

        public void ExecuteState()
        {
            Debug.Log("[DefaultCombatState] 상태 실행 중 (기본 상태)");
            // 필요 시 자동 전이 가능: turnManager.RequestStateChange(...);
        }

        public void ExitState()
        {
            Debug.Log("[DefaultCombatState] 상태 종료");
        }
    }
}
