using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;

namespace Game.CombatSystem.State
{
    public class CombatSecondAttackState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICoroutineRunner coroutineRunner;

        public CombatSecondAttackState(
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
            Debug.Log("[CombatSecondAttackState] 상태 진입");

            flowCoordinator.DisablePlayerInput();

            coroutineRunner.RunCoroutine(SecondAttackRoutine());
        }

        private IEnumerator SecondAttackRoutine()
        {
            yield return flowCoordinator.PerformSecondAttack();

            var next = turnManager.GetStateFactory().CreateResultState();
            turnManager.RequestStateChange(next);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatSecondAttackState] 상태 종료");
        }
    }
}
