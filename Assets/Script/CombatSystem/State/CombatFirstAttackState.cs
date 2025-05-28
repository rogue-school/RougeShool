using Game.CombatSystem.Interface;
using UnityEngine;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatFirstAttackState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;

        public CombatFirstAttackState(
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
            Debug.Log("[State] CombatFirstAttackState: 선공 슬롯 전투 시작");

            if (flowCoordinator is MonoBehaviour mono)
            {
                mono.StartCoroutine(AttackRoutine());
            }
            else
            {
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
            }
        }

        private IEnumerator AttackRoutine()
        {
            yield return flowCoordinator.PerformFirstAttack();

            var nextState = stateFactory.CreateSecondAttackState();
            turnManager.RequestStateChange(nextState);
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
