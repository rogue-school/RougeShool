using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;
using System.Collections;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatVictoryState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;
        private readonly ICombatSlotRegistry slotRegistry;

        public CombatVictoryState(
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
            Debug.Log("[State] CombatVictoryState: 승리 처리 시작");

            if (flowCoordinator is MonoBehaviour mono)
                mono.StartCoroutine(VictoryRoutine());
            else
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아님");
        }

        private IEnumerator VictoryRoutine()
        {
            yield return flowCoordinator.PerformVictoryPhase();

            if (flowCoordinator.CheckHasNextEnemy())
            {
                Debug.Log("[State] 다음 적이 있어 PrepareState로 전이");
                turnManager.RequestStateChange(stateFactory.CreatePrepareState());
            }
            else
            {
                Debug.Log("[State] 전투 종료, Cleanup 실행");
                flowCoordinator.CleanupAfterVictory();
            }
        }

        public void ExecuteState() { }
        public void ExitState() { }
    }
}
