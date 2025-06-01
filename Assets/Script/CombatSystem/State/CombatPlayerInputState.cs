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
            // 플레이어의 입력을 대기하는 상태이므로, 이곳에는 실행 로직 없음
            // 턴 시작 버튼이 눌리면 TurnStateController가 상태 전이 요청함
        }

        public void ExitState()
        {
            Debug.Log("[CombatPlayerInputState] 종료");

            flowCoordinator.DisablePlayerInput();
        }
        public class Factory : Zenject.PlaceholderFactory<CombatPlayerInputState> { }
    }
}
