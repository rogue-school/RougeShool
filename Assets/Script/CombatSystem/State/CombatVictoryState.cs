using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.Utility;
using UnityEngine;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatVictoryState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatSlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;

        public CombatVictoryState(
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
            Debug.Log("[CombatVictoryState] 상태 진입 - 승리 처리 시작");
            coroutineRunner.RunCoroutine(VictoryRoutine());
        }

        private IEnumerator VictoryRoutine()
        {
            yield return flowCoordinator.PerformVictoryPhase();

            if (flowCoordinator.CheckHasNextEnemy())
            {
                Debug.Log("[CombatVictoryState] 다음 적이 있어 PrepareState로 전이");
                turnManager.RequestStateChange(turnManager.GetStateFactory().CreatePrepareState());
            }
            else
            {
                Debug.Log("[CombatVictoryState] 최종 승리, 전투 정리 시작");
                flowCoordinator.CleanupAfterVictory();
            }
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
