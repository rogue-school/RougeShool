using UnityEngine;
using Game.Managers;
using Game.Combat.Turn;

namespace Game.Combat.Turn
{
    /// <summary>
    /// 기본 컴뱃 턴 처리 상태입니다.
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
            Debug.Log("기본 상태 진입: DefaultCombatState");
        }

        public void ExecuteState()
        {
            // 기본 턴 로직 실행
            Debug.Log("기본 전투 턴이 실행되었습니다.");

            // 예시: 상태 전환
            // turnManager.SetState(new PlayerGuardedState(turnManager));
        }

        public void ExitState()
        {
            Debug.Log("기본 상태 종료: DefaultCombatState");
        }
    }
}
