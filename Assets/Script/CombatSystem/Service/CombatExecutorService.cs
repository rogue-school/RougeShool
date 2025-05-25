using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Utility;
using Game.SkillCardSystem.Executor;

namespace Game.CombatSystem.Executor
{
    public class CombatExecutorService : ICombatExecutor
    {
        private readonly ISlotRegistry slotRegistry;
        private readonly IEnemyHandManager enemyHandManager;

        private ICardExecutionContextProvider contextProvider;
        private ICardExecutor cardExecutor;
        private ITurnStateController turnController;

        public CombatExecutorService(
            ISlotRegistry slotRegistry,
            ICardExecutionContextProvider contextProvider,
            ICardExecutor cardExecutor,
            IEnemyHandManager enemyHandManager)
        {
            this.slotRegistry = slotRegistry;
            this.contextProvider = contextProvider;
            this.cardExecutor = cardExecutor;
            this.enemyHandManager = enemyHandManager;
        }

        public void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor)
        {
            this.contextProvider = provider;
            this.cardExecutor = executor;
        }

        public void SetTurnController(ITurnStateController controller)
        {
            this.turnController = controller;
        }

        public IEnumerator PerformAttack(CombatSlotPosition slotPosition)
        {
            var slot = slotRegistry.GetCombatSlot(SlotPositionUtil.ToFieldSlot(slotPosition));

            if (slot == null || slot.IsEmpty())
            {
                Debug.LogWarning($"[Executor] 슬롯 {slotPosition} 에 카드가 없습니다.");
                yield break;
            }

            var card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning($"[Executor] 슬롯 {slotPosition} 에 등록된 카드가 null입니다.");
                yield break;
            }

            Debug.Log($"[Executor] 카드 실행 시작: {card.GetCardName()}");

            var context = contextProvider.CreateContext(card);
            cardExecutor?.Execute(card, context, turnController);

            yield return new WaitForSeconds(0.5f);

            slot.Clear();
            Debug.Log($"[Executor] 슬롯 {slotPosition} 클리어 완료");
        }
    }
}
