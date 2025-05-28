using Game.CombatSystem.Interface;
using UnityEngine;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatGameOverState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;

        public CombatGameOverState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatStateFactory stateFactory)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[State] CombatGameOverState: 게임 오버 처리 시작");

            if (flowCoordinator is MonoBehaviour mono)
            {
                mono.StartCoroutine(GameOverRoutine());
            }
            else
            {
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
            }
        }

        private IEnumerator GameOverRoutine()
        {
            yield return flowCoordinator.PerformGameOverPhase();

            // 게임 오버 이후 씬 전환 또는 UI 처리 예정
            Debug.Log("[State] CombatGameOverState: 게임 오버 처리 완료");
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
