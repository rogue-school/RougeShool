using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatSecondAttackState : ICombatTurnState
    {
        private readonly ITurnStateController controller;
        private readonly ICombatSlotManager slotManager;
        private readonly ICardExecutionContext context;
        private readonly ICombatStateFactory stateFactory;

        public CombatSecondAttackState(
            ITurnStateController controller,
            ICombatSlotManager slotManager,
            ICardExecutionContext context,
            ICombatStateFactory stateFactory)
        {
            this.controller = controller;
            this.slotManager = slotManager;
            this.context = context;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[CombatSecondAttackState] 후공 턴 시작");

            var secondSlot = slotManager.GetSlot(CombatSlotPosition.SECOND);
            if (secondSlot == null || !secondSlot.HasCard())
            {
                Debug.LogWarning("[CombatSecondAttackState] 후공 슬롯이 비어 있음 → 결과 상태로 전이");
                controller.RequestStateChange(stateFactory.CreateResultState());
                return;
            }

            ISkillCard secondCard = secondSlot.GetCard();
            var caster = secondCard.GetOwner(context);
            var target = secondCard.GetTarget(context);

            if (target == null || target.IsDead())
            {
                Debug.Log("[CombatSecondAttackState] 타겟이 사망하여 실행 생략");
            }
            else
            {
                secondSlot.ExecuteCardAutomatically(context);
            }

            controller.RequestStateChange(stateFactory.CreateResultState());
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatSecondAttackState] 후공 턴 종료");
        }
    }
}
