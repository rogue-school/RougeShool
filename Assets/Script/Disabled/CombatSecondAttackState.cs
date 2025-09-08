using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;
using Game.CoreSystem.Utility;
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
            // 애니메이션 락이 걸려 있으면 대기 또는 return
            if (AnimationSystem.Manager.AnimationFacade.Instance.IsHandVanishAnimationPlaying)
            {
                Debug.Log("[SecondAttackState] 핸드 소멸 애니메이션 진행 중이므로 상태 전이 대기");
                return;
            }
            flowCoordinator.DisablePlayerInput();

            bool enemyDead = turnContext.WasEnemyDefeated;
            bool playerDead = flowCoordinator.IsPlayerDead();

            // 1. 이미 소멸 애니메이션이 실행된 경우 바로 상태 전이
            if (turnContext.WasHandCardsVanishedThisTurn)
            {
                Debug.Log("<color=cyan>[STATE] CombatSecondAttackState → CombatResultState 전이 (이미 소멸 애니메이션 실행됨)");
                var next = turnManager.GetStateFactory().CreateResultState();
                turnManager.RequestStateChange(next);
                ((CombatTurnManager)turnManager).ApplyPendingState();
                return;
            }

            // 2. 사망 시 소멸 애니메이션 먼저 실행
            if (enemyDead)
            {
                AnimationSystem.Manager.AnimationFacade.Instance.VanishAllHandCardsOnCharacterDeath(false, () =>
                {
                    turnContext.MarkHandCardsVanished();
                    Debug.Log("<color=cyan>[STATE] CombatSecondAttackState → CombatResultState 전이 (적 사망 후 소멸 애니메이션 완료)");
                    var next = turnManager.GetStateFactory().CreateResultState();
                    turnManager.RequestStateChange(next);
                    ((CombatTurnManager)turnManager).ApplyPendingState();
                });
                return;
            }
            if (playerDead)
            {
                AnimationSystem.Manager.AnimationFacade.Instance.VanishAllHandCardsOnCharacterDeath(true, () =>
                {
                    turnContext.MarkHandCardsVanished();
                    Debug.Log("<color=cyan>[STATE] CombatSecondAttackState → CombatResultState 전이 (플레이어 사망 후 소멸 애니메이션 완료)");
                    var next = turnManager.GetStateFactory().CreateResultState();
                    turnManager.RequestStateChange(next);
                    ((CombatTurnManager)turnManager).ApplyPendingState();
                });
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
