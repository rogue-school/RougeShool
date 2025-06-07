using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;

namespace Game.CombatSystem.State
{
    public class CombatVictoryState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICoroutineRunner coroutineRunner;

        public CombatVictoryState(
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
            Debug.Log("<color=magenta>[CombatVictoryState] 승리 상태 진입</color>");
            coroutineRunner.RunCoroutine(HandleVictory());
        }

        private IEnumerator HandleVictory()
        {
            yield return flowCoordinator.PerformVictoryPhase();

            flowCoordinator.CleanupAfterVictory();

            if (flowCoordinator.CheckHasNextEnemy())
            {
                var next = turnManager.GetStateFactory().CreatePrepareState();
                turnManager.RequestStateChange(next);
            }
            else
            {
                var next = turnManager.GetStateFactory().CreateGameOverState();
                turnManager.RequestStateChange(next);
            }
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatVictoryState] 상태 종료");
        }
    }
}
