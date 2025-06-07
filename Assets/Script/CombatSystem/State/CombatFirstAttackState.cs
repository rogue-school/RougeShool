using Game.CombatSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.State
{
    public class CombatFirstAttackState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatSlotRegistry slotRegistry;

        public CombatFirstAttackState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatSlotRegistry slotRegistry)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.slotRegistry = slotRegistry;
        }

        public void EnterState()
        {
            Debug.Log("[CombatFirstAttackState] 상태 진입");

            flowCoordinator.RequestFirstAttack(() =>
            {
                var next = turnManager.GetStateFactory().CreateSecondAttackState();
                turnManager.RequestStateChange(next);
            });
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
