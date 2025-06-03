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

            flowCoordinator.DisableStartButton(); // 초기 비활성화만 수행

            // 카드 등록 시 TurnStartButtonHandler의 이벤트로 버튼 상태가 자동 평가됨
        }

        public void ExecuteState()
        {
            // 플레이어 입력을 대기하는 상태
        }

        public void ExitState()
        {
            Debug.Log("[CombatPlayerInputState] 종료");

            flowCoordinator.DisablePlayerInput();
            flowCoordinator.DisableStartButton(); // 종료 시 항상 비활성화
        }

        public class Factory : Zenject.PlaceholderFactory<CombatPlayerInputState> { }
    }
}
