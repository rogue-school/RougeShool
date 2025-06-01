using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.Utility;
using UnityEngine;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatGameOverState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatSlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;

        public CombatGameOverState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatSlotRegistry slotRegistry,
            ICoroutineRunner coroutineRunner)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.slotRegistry = slotRegistry;
            this.coroutineRunner = coroutineRunner;
        }

        public void EnterState()
        {
            Debug.Log("[CombatGameOverState] 상태 진입 - 게임 오버 처리 시작");
            coroutineRunner.RunCoroutine(GameOverRoutine());
        }

        private IEnumerator GameOverRoutine()
        {
            yield return flowCoordinator.PerformGameOverPhase();

            // TODO: 게임 오버 후 UI나 씬 전환 처리
            Debug.Log("[CombatGameOverState] 게임 오버 처리 완료");
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
