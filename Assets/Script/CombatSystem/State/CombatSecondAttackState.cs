using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;
using Game.CombatSystem.Context;
using Game.CombatSystem.Manager;

namespace Game.CombatSystem.State
{
    public class CombatSecondAttackState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly TurnContext turnContext;

        public CombatSecondAttackState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICoroutineRunner coroutineRunner,
            TurnContext turnContext)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.coroutineRunner = coroutineRunner;
            this.turnContext = turnContext;
        }

        public void EnterState()
        {
            Debug.Log("<color=cyan>[STATE] CombatSecondAttackState 진입</color>");
            flowCoordinator.DisablePlayerInput();

            if (turnContext.WasEnemyDefeated || flowCoordinator.IsPlayerDead())
            {
                Debug.Log("<color=cyan>[STATE] CombatSecondAttackState → CombatResultState 전이 (적 사망 또는 플레이어 사망)</color>");
                var next = turnManager.GetStateFactory().CreateResultState();
                turnManager.RequestStateChange(next);

                // 즉시 상태 전이를 실행
                ((CombatTurnManager)turnManager).ApplyPendingState();
                return;
            }

            coroutineRunner.RunCoroutine(SecondAttackRoutine());
        }

        private IEnumerator SecondAttackRoutine()
        {
            yield return flowCoordinator.PerformSecondAttack();

            Debug.Log("<color=cyan>[STATE] CombatSecondAttackState → CombatResultState 전이</color>");
            var next = turnManager.GetStateFactory().CreateResultState();
            turnManager.RequestStateChange(next);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=cyan>[STATE] CombatSecondAttackState 종료</color>");
        }
    }
}
