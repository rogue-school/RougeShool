using Game.CombatSystem.Interface;
using UnityEngine;
using System.Collections;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;

        public CombatResultState(
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
            Debug.Log("[State] CombatResultState: 전투 결과 판단 시작");

            if (flowCoordinator is MonoBehaviour mono)
                mono.StartCoroutine(ResultRoutine());
            else
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
        }

        private IEnumerator ResultRoutine()
        {
            yield return flowCoordinator.PerformResultPhase();

            if (flowCoordinator.IsPlayerDead())
            {
                Debug.Log("[State] 플레이어 사망 → GameOver 상태로 전이");
                turnManager.RequestStateChange(stateFactory.CreateGameOverState());
            }
            else if (flowCoordinator.IsEnemyDead())
            {
                if (flowCoordinator.CheckHasNextEnemy())
                {
                    Debug.Log("[State] 적 사망 + 다음 적 존재 → Prepare 상태 전이");
                    turnManager.RequestStateChange(stateFactory.CreatePrepareState());
                }
                else
                {
                    Debug.Log("[State] 적 사망 + 다음 적 없음 → Victory 상태 전이");
                    turnManager.RequestStateChange(stateFactory.CreateVictoryState());
                }
            }
            else
            {
                Debug.Log("[State] 적 생존 → 다음 턴 준비");
                turnManager.RequestStateChange(stateFactory.CreatePrepareState());
            }
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}

