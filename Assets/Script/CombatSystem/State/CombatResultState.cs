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
            Debug.Log("[CombatResultState] 상태 진입");

            flowCoordinator.DisablePlayerInput();
            coroutineRunner.RunCoroutine(ExecuteResultPhase());
        }

        private IEnumerator ExecuteResultPhase()
        {
            yield return flowCoordinator.PerformResultPhase();

            if (flowCoordinator.IsEnemyDead())
            {
                var next = turnManager.GetStateFactory().CreateVictoryState();
                turnManager.RequestStateChange(next);
            }
            else if (flowCoordinator.IsPlayerDead())
            {
                var next = turnManager.GetStateFactory().CreateGameOverState();
                turnManager.RequestStateChange(next);
            }
            else
            {
                var next = turnManager.GetStateFactory().CreatePlayerInputState();
                turnManager.RequestStateChange(next);
            }
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatResultState] 상태 종료");
        }
    }
}
