using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;
using System.Collections;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;
        private readonly ISlotRegistry slotRegistry;

        public CombatPrepareState(
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
            Debug.Log("[State] CombatPrepareState: 적 생성 및 슬롯 준비 시작");

            // flowCoordinator가 MonoBehaviour 기반이므로 StartCoroutine 사용 가능
            if (flowCoordinator is MonoBehaviour mono)
            {
                mono.StartCoroutine(PrepareRoutine());
            }
            else
            {
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
            }
        }

        private IEnumerator PrepareRoutine()
        {
            yield return flowCoordinator.PerformCombatPreparation();

            Debug.Log("[State] CombatPrepareState: 준비 완료, 입력 상태로 전이");
            var nextState = stateFactory.CreatePlayerInputState();
            turnManager.RequestStateChange(nextState);
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
