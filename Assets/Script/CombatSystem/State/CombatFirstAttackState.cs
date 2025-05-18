using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatFirstAttackState : ICombatTurnState
    {
        private readonly ITurnStateController controller;
        private readonly ICombatSlotManager slotManager;
        private readonly ICardExecutionContext executionContext;
        private readonly ICombatStateFactory stateFactory;

        public CombatFirstAttackState(
            ITurnStateController controller,
            ICombatSlotManager slotManager,
            ICardExecutionContext executionContext,
            ICombatStateFactory stateFactory)
        {
            this.controller = controller;
            this.slotManager = slotManager;
            this.executionContext = executionContext;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[CombatFirstAttackState] 선공 턴 시작");

            var firstSlot = slotManager.GetSlot(CombatSlotPosition.FIRST);
            if (firstSlot == null || !firstSlot.HasCard())
            {
                Debug.LogWarning("[CombatFirstAttackState] 선공 슬롯이 비어 있음 → 후공 상태로 전이");
                controller.RequestStateChange(stateFactory.CreateSecondAttackState());
                return;
            }

            ISkillCard firstCard = firstSlot.GetCard();

            firstSlot.ExecuteCardAutomatically(executionContext);

            if (executionContext.GetPlayer().IsDead())
            {
                Debug.Log("[CombatFirstAttackState] 플레이어 사망 → GameOverState");
                controller.RequestStateChange(stateFactory.CreateGameOverState());
                return;
            }

            if (executionContext.GetEnemy().IsDead())
            {
                Debug.Log("[CombatFirstAttackState] 적 사망 감지");
            }

            controller.RequestStateChange(stateFactory.CreateSecondAttackState());
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatFirstAttackState] 선공 턴 종료");
        }
    }
}
