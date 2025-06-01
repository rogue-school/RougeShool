using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.Utility;
using UnityEngine;
using System.Collections;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatSlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;

        public CombatResultState(
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
            Debug.Log("[CombatResultState] 상태 진입");
            coroutineRunner.RunCoroutine(ResultRoutine());
        }

        private IEnumerator ResultRoutine()
        {
            yield return flowCoordinator.PerformResultPhase();

            var factory = turnManager.GetStateFactory();

            if (flowCoordinator.IsPlayerDead())
            {
                turnManager.RequestStateChange(factory.CreateGameOverState());
            }
            else if (flowCoordinator.IsEnemyDead())
            {
                turnManager.RequestStateChange(
                    flowCoordinator.CheckHasNextEnemy()
                        ? factory.CreatePrepareState()
                        : factory.CreateVictoryState()
                );
            }
            else
            {
                turnManager.RequestStateChange(factory.CreatePrepareState());
            }
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
