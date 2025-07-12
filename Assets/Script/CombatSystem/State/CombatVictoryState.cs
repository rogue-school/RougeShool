using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;
using Game.CombatSystem;

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
            CombatEvents.Result.RaiseVictory();
            coroutineRunner.RunCoroutine(HandleVictory());
        }

        private IEnumerator HandleVictory()
        {
            yield return flowCoordinator.PerformVictoryPhase();

            yield return flowCoordinator.CleanupAfterVictory();

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
        }
    }
}
