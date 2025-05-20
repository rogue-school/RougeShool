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
        private readonly ISlotRegistry slotRegistry;

        public CombatVictoryState(
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
            Debug.Log("[State] CombatVictoryState: 승리 처리 시작");

            if (flowCoordinator is MonoBehaviour mono)
            {
                mono.StartCoroutine(VictoryRoutine());
            }
            else
            {
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
            }
        }

        private IEnumerator VictoryRoutine()
        {
            yield return flowCoordinator.PerformVictoryPhase();

            // 다음 적이 있으면 PrepareState, 없으면 현재 전투 종료
            if (flowCoordinator.CheckHasNextEnemy())
            {
                Debug.Log("[State] 다음 적이 있어 PrepareState로 전이");
                var nextState = stateFactory.CreatePrepareState();
                turnManager.RequestStateChange(nextState);
            }
            else
            {
                Debug.Log("[State] 전투 종료, Victory 상태 종료");
                // TODO: 전투 UI 종료 처리 등
            }
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
