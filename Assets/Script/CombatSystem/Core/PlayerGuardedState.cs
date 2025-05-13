using UnityEngine;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 플레이어가 가드 상태일 때의 컴뱃 턴 상태입니다.
    /// </summary>
    public class PlayerGuardedState : ICombatTurnState
    {
        private readonly CombatTurnManager turnManager;

        public PlayerGuardedState(CombatTurnManager manager)
        {
            turnManager = manager;
        }

        public void EnterState()
        {
            Debug.Log("[PlayerGuardedState] 진입 - 플레이어가 방어 상태입니다.");
        }

        public void ExecuteState()
        {
            Debug.Log("[PlayerGuardedState] 실행 - 방어 상태에서 턴이 처리됩니다.");
            // 방어 처리 로직 등...

            // 다음 상태로 전환
            turnManager.SetState(new DefaultCombatState(turnManager));
        }

        public void ExitState()
        {
            Debug.Log("[PlayerGuardedState] 종료 - 다음 상태로 전환됩니다.");
        }
    }
}
