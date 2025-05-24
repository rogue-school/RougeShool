using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Executor;

namespace Game.CombatSystem.Service
{
    public class CombatExecutorService : ICombatExecutor
    {
        private readonly ISlotRegistry slotRegistry;
        private readonly ICardExecutionContextProvider contextProvider;
        private readonly ICardExecutor cardExecutor;
        private readonly IEnemyHandManager enemyHandManager;

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

        public IEnumerator PerformAttack(CombatSlotPosition slotPosition)
        {
            var slot = slotRegistry.GetCombatSlot(slotPosition);
            var card = slot?.GetCard();

            if (card != null)
            {
                var context = contextProvider.CreateContext(card);
                cardExecutor.Execute(card, context);
            }
            else
            {
                Debug.LogWarning($"[CombatExecutor] 슬롯 {slotPosition}에 카드가 없습니다.");
            }

            yield return new WaitForSeconds(0.5f);

            slot?.Clear();
            enemyHandManager.AdvanceSlots();
        }
    }
}
