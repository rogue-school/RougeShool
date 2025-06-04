using UnityEngine;
using System.Collections;
using Zenject;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.Utility;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ITurnCardRegistry cardRegistry;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly ICoolTimeHandler coolTimeHandler;

        private bool isStateEntered;

        [Inject]
        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ITurnCardRegistry cardRegistry,
            ICoroutineRunner coroutineRunner,
            ICoolTimeHandler coolTimeHandler)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.cardRegistry = cardRegistry;
            this.coroutineRunner = coroutineRunner;
            this.coolTimeHandler = coolTimeHandler;
        }

        public void EnterState()
        {
            if (isStateEntered)
            {
                Debug.LogWarning("[CombatPrepareState] 이미 상태에 진입했습니다. 중복 호출 방지됨");
                return;
            }

            isStateEntered = true;

            Debug.Log("<color=yellow>[CombatPrepareState] 상태 진입</color>");

            flowCoordinator.DisablePlayerInput();

            // 적 카드만 초기화 (플레이어 카드 보존)
            cardRegistry.ClearEnemyCardsOnly();

            coroutineRunner.RunCoroutine(PrepareRoutine());
        }

        private IEnumerator PrepareRoutine()
        {
            coolTimeHandler.ReduceCoolTimes();
            yield return flowCoordinator.PerformCombatPreparation();

            Debug.Log("<color=lime>[CombatPrepareState] 적 전투 준비 완료 → 플레이어 입력 상태로 이동</color>");
            var nextState = turnManager.GetStateFactory().CreatePlayerInputState();
            turnManager.RequestStateChange(nextState);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatPrepareState] 상태 종료</color>");
            isStateEntered = false;
        }
    }
}
