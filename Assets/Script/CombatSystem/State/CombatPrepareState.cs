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

        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatStateFactory stateFactory)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[State] CombatPrepareState: 적 생성 및 슬롯 준비 시작");

            if (flowCoordinator is MonoBehaviour mono)
                mono.StartCoroutine(PrepareRoutine());
            else
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
        }

        private IEnumerator PrepareRoutine()
        {
            bool prepareSuccess = false;
            yield return flowCoordinator.PerformCombatPreparation(success => prepareSuccess = success);

            if (!prepareSuccess)
            {
                Debug.LogError("[State] CombatPrepareState: 준비 실패 → 상태 전이 중단");
                yield break;
            }

            Debug.Log("[State] CombatPrepareState: 준비 완료, 입력 상태로 전이");
            turnManager.RequestStateChange(stateFactory.CreatePlayerInputState());
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[State] CombatPrepareState: 상태 종료");
        }
    }
}
