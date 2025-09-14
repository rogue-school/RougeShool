using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
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
            Debug.Log("<color=cyan>[STATE] CombatVictoryState 진입</color>");
            CombatEvents.RaiseVictory();
            coroutineRunner.RunCoroutine(HandleVictory());
        }

        private IEnumerator HandleVictory()
        {
            yield return flowCoordinator.PerformVictoryPhase();

            yield return flowCoordinator.CleanupAfterVictory();

            if (flowCoordinator.CheckHasNextEnemy())
            {
                Debug.Log("<color=cyan>[STATE] CombatVictoryState → CombatPrepareState 전이 (다음 적 존재)</color>");
                var next = turnManager.GetStateFactory().CreatePrepareState();
                turnManager.RequestStateChange(next);
            }
            else
            {
                Debug.Log("<color=cyan>[STATE] CombatVictoryState → CombatGameOverState 전이 (모든 적 처치 완료)</color>");
                var next = turnManager.GetStateFactory().CreateGameOverState();
                turnManager.RequestStateChange(next);
            }
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=cyan>[STATE] CombatVictoryState 종료</color>");
        }
    }
}
