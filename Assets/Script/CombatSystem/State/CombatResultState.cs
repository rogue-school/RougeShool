using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Manager;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ICombatTurnManager controller;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;

        public CombatResultState(
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
            Debug.Log("[CombatResultState] 상태 진입 - 결과 처리 시작");

            if (controller is MonoBehaviour behaviour)
                behaviour.StartCoroutine(HandleResultPhase());
            else
                Debug.LogError("[CombatResultState] controller는 MonoBehaviour가 아닙니다.");
        }

        private IEnumerator HandleResultPhase()
        {
            yield return flowCoordinator.PerformResultPhase();

            // 전투 승리 조건 확인
            bool hasMoreEnemies = flowCoordinator.CheckHasNextEnemy(); // 이 메서드는 새로 정의 필요
            ICombatTurnState nextState;

            if (hasMoreEnemies)
                nextState = stateFactory.CreatePrepareState();
            else if (flowCoordinator.IsPlayerDead()) // 이 메서드도 마찬가지로 flowCoordinator에 위임 가능
                nextState = stateFactory.CreateGameOverState();
            else
                nextState = stateFactory.CreateVictoryState();

            controller.RequestStateChange(nextState);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatResultState] 상태 종료");
        }
    }
}
