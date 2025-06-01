using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;
        private readonly ICombatSlotRegistry slotRegistry;

        private bool isStateEntered = false;

        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatStateFactory stateFactory,
            ICombatSlotRegistry slotRegistry)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.stateFactory = stateFactory;
            this.slotRegistry = slotRegistry;
        }

        public void EnterState()
        {
            if (isStateEntered) return;

            isStateEntered = true;
            Debug.Log("[CombatPrepareState] 진입");

            flowCoordinator.DisablePlayerInput();
            flowCoordinator.RequestCombatPreparation(OnPrepareComplete);
        }

        private void OnPrepareComplete(bool success)
        {
            if (!success)
            {
                Debug.LogError("[CombatPrepareState] 전투 준비 실패 → 게임 오버 상태로 전환");

                var failState = stateFactory.CreateGameOverState();
                turnManager.RequestStateChange(failState);
                return;
            }

            Debug.Log("[CombatPrepareState] 전투 준비 완료 → 플레이어 입력 상태로 전환");

            var next = stateFactory.CreatePlayerInputState();
            turnManager.RequestStateChange(next);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatPrepareState] 종료");
            isStateEntered = false;
        }
    }
}
