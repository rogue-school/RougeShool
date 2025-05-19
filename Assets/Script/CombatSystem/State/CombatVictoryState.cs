using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Manager;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatVictoryState : ICombatTurnState
    {
        private readonly ICombatTurnManager controller;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;

        public CombatVictoryState(
            ICombatTurnManager controller,
            ICombatFlowCoordinator flowCoordinator,
            ICombatStateFactory stateFactory)
        {
            this.controller = controller;
            this.flowCoordinator = flowCoordinator;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[CombatVictoryState] 상태 진입 - 승리 처리 및 다음 적 준비");

            if (controller is MonoBehaviour mono)
                mono.StartCoroutine(HandleVictory());
            else
                Debug.LogError("[CombatVictoryState] controller가 MonoBehaviour를 구현하지 않아 코루틴 실행 불가");
        }

        private IEnumerator HandleVictory()
        {
            yield return flowCoordinator.PerformVictoryPhase();

            bool hasNextEnemy = flowCoordinator.CheckHasNextEnemy();
            ICombatTurnState nextState;

            if (hasNextEnemy)
            {
                nextState = stateFactory.CreatePrepareState();
                Debug.Log("[CombatVictoryState] 다음 적 존재 → Prepare 상태로 전이");
            }
            else
            {
                Debug.Log("[CombatVictoryState] 모든 적 처치 → 스테이지 종료 처리 가능");
                // 다음 스테이지 전환 또는 결과 화면 등으로 이동 가능
                // 여기서 상태 전이를 생략하거나 외부 시스템으로 넘길 수 있음
                yield break;
            }

            controller.RequestStateChange(nextState);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatVictoryState] 상태 종료");
        }
    }
}
