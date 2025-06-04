using UnityEngine;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.State
{
    public class CombatPlayerInputState : ICombatTurnState
    {
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ITurnCardRegistry cardRegistry;
        private readonly ICombatTurnManager turnManager;

        private bool hasStarted = false;

        public CombatPlayerInputState(
            ICombatFlowCoordinator flowCoordinator,
            ITurnCardRegistry cardRegistry,
            ICombatTurnManager turnManager)
        {
            this.flowCoordinator = flowCoordinator;
            this.cardRegistry = cardRegistry;
            this.turnManager = turnManager;
        }

        public void EnterState()
        {
            Debug.Log("<color=cyan>[CombatPlayerInputState] 상태 진입</color>");
            hasStarted = false;

            flowCoordinator.EnablePlayerInput();
            flowCoordinator.ShowPlayerCardSelectionUI();
            flowCoordinator.DisableStartButton();

            flowCoordinator.RegisterStartButton(OnStartButtonPressed);
        }

        private void OnStartButtonPressed()
        {
            if (hasStarted)
            {
                Debug.LogWarning("[CombatPlayerInputState] 이미 시작 버튼이 눌렸습니다.");
                return;
            }

            hasStarted = true;

            Debug.Log("[CombatPlayerInputState] 플레이어 입력 완료 → 전투 시작");

            flowCoordinator.DisableStartButton();
            flowCoordinator.DisablePlayerInput();
            flowCoordinator.HidePlayerCardSelectionUI();
            flowCoordinator.UnregisterStartButton();

            var next = turnManager.GetStateFactory().CreateFirstAttackState();
            turnManager.RequestStateChange(next);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatPlayerInputState] 상태 종료</color>");

            flowCoordinator.DisablePlayerInput();
            flowCoordinator.HidePlayerCardSelectionUI();
            flowCoordinator.UnregisterStartButton();
            flowCoordinator.DisableStartButton();
        }
    }
}
