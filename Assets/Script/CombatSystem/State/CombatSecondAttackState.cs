using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;
using System.Collections;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatSecondAttackState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;

        public CombatSecondAttackState(
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
            Debug.Log("[State] CombatSecondAttackState: 후공 슬롯 전투 시작");

            if (flowCoordinator is MonoBehaviour mono)
                mono.StartCoroutine(AttackRoutine());
            else
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
        }

        private IEnumerator AttackRoutine()
        {
            yield return flowCoordinator.PerformSecondAttack();

            Debug.Log("[State] 후공 전투 완료 → Result 상태로 전이");
            turnManager.RequestStateChange(stateFactory.CreateResultState());
        }

        public void ExecuteState() { }
        public void ExitState() { }
    }
}
