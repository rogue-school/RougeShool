using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICoroutineRunner coroutineRunner;

        public CombatResultState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICoroutineRunner coroutineRunner)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.coroutineRunner = coroutineRunner;
        }

        public void EnterState()
        {
            Debug.Log("<color=orange>[CombatResultState] 상태 진입</color>");
            flowCoordinator.DisablePlayerInput();
            coroutineRunner.RunCoroutine(ExecuteResultPhase());
        }

        private IEnumerator ExecuteResultPhase()
        {
            yield return flowCoordinator.PerformResultPhase();

            if (flowCoordinator.IsEnemyDead())
            {
                Debug.Log("[CombatResultState] 적 사망 → 적 정보 제거");

                flowCoordinator.RemoveEnemyCharacter();
                flowCoordinator.ClearEnemyHand();

                if (flowCoordinator.CheckHasNextEnemy())
                {
                    Debug.Log("[CombatResultState] 다음 적 있음 → PrepareState 전환");
                    flowCoordinator.SpawnNextEnemy();
                    var next = turnManager.GetStateFactory().CreatePrepareState();
                    turnManager.RequestStateChange(next);
                }
                else
                {
                    Debug.Log("[CombatResultState] VictoryState 전환");
                    var next = turnManager.GetStateFactory().CreateVictoryState();
                    turnManager.RequestStateChange(next);
                }
            }
            else if (flowCoordinator.IsPlayerDead())
            {
                Debug.Log("[CombatResultState] 플레이어 사망 → GameOverState 전환");
                var next = turnManager.GetStateFactory().CreateGameOverState();
                turnManager.RequestStateChange(next);
            }
            else
            {
                Debug.Log("[CombatResultState] 전투 계속 → PrepareState 전환");
                var next = turnManager.GetStateFactory().CreatePrepareState();
                turnManager.RequestStateChange(next);
            }
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatResultState] 상태 종료</color>");
        }
    }
}
