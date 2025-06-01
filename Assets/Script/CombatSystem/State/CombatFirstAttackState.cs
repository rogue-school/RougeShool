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
        private readonly ICombatSlotRegistry slotRegistry;

        public CombatFirstAttackState(
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
            flowCoordinator.RequestFirstAttack(() =>
            {
                var next = stateFactory.CreateSecondAttackState();
                turnManager.RequestStateChange(next);
            });
        }


        public void ExecuteState() { }

        public void ExitState() { }
    }

}
