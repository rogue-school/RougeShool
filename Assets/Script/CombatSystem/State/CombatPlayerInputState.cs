using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;

namespace Game.CombatSystem.State
{
    public class CombatPlayerInputState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;
        private readonly ICombatSlotRegistry slotRegistry;

        public CombatPlayerInputState(
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
            Debug.Log("[CombatPlayerInputState] 진입");

            flowCoordinator.EnablePlayerInput();
        }

        public void ExecuteState()
        {
            // 입력 대기 중일 뿐이므로 로직 없음
        }

        public void ExitState()
        {
            Debug.Log("[CombatPlayerInputState] 종료");

            flowCoordinator.DisablePlayerInput();
        }
    }
}
