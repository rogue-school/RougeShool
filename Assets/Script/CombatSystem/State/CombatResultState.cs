using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;
        private readonly ICombatSlotRegistry slotRegistry;

        public CombatResultState(
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
            if (flowCoordinator is MonoBehaviour mono)
                mono.StartCoroutine(ResultRoutine());
            else
                Debug.LogError("Coroutine 실행 불가: flowCoordinator가 MonoBehaviour가 아님");
        }

        private IEnumerator ResultRoutine()
        {
            yield return flowCoordinator.PerformResultPhase();

            if (flowCoordinator.IsPlayerDead())
            {
                turnManager.RequestStateChange(stateFactory.CreateGameOverState());
            }
            else if (flowCoordinator.IsEnemyDead())
            {
                turnManager.RequestStateChange(flowCoordinator.CheckHasNextEnemy()
                    ? stateFactory.CreatePrepareState()
                    : stateFactory.CreateVictoryState());
            }
            else
            {
                turnManager.RequestStateChange(stateFactory.CreatePrepareState());
            }
        }

        public void ExecuteState() { }
        public void ExitState() { }
    }
}
