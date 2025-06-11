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
            Debug.Log("[CombatSecondAttackState] 상태 진입");

            flowCoordinator.DisablePlayerInput();

            if (turnContext.WasEnemyDefeated || flowCoordinator.IsPlayerDead())
            {
                Debug.Log("[CombatSecondAttackState] 적 또는 플레이어가 죽었으므로 공격 생략");

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
