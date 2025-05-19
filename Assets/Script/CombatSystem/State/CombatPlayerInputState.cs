using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.IManager;
using UnityEngine;

namespace Game.CombatSystem.State
{
    public class CombatPlayerInputState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;
        private readonly ISlotRegistry slotRegistry;

        public CombatPlayerInputState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatStateFactory stateFactory,
            ISlotRegistry slotRegistry)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.stateFactory = stateFactory;
            this.slotRegistry = slotRegistry;
        }

        public void EnterState()
        {
            flowCoordinator.EnablePlayerInput(); // 드래그 등 허용

            Debug.Log("[State] CombatPlayerInputState: 플레이어 입력 가능 상태 진입");
            // 상태 전이는 플레이어가 버튼 클릭 시 trigger됨
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            flowCoordinator.DisablePlayerInput(); // 드래그 등 비활성화
            Debug.Log("[State] CombatPlayerInputState: 입력 종료");
        }
    }
}
