using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Executor
{
    /// <summary>
    /// 전투 슬롯에 등록된 스킬 카드를 실행하는 서비스입니다.
    /// SRP: 카드 실행만 책임집니다.
    /// DIP: ICombatSlotRegistry, ICardExecutor, ICardExecutionContextProvider 인터페이스에 의존합니다.
    /// </summary>
    public class CombatExecutorService : ICombatExecutor
    {
        private readonly ICombatSlotRegistry combatSlotRegistry;
        private readonly IEnemyHandManager enemyHandManager;

        private ICardExecutionContextProvider contextProvider;
        private ICardExecutor cardExecutor;
        private ITurnStateController turnController;

        public CombatExecutorService(
            ICombatSlotRegistry combatSlotRegistry,
            ICardExecutionContextProvider contextProvider,
            ICardExecutor cardExecutor,
            IEnemyHandManager enemyHandManager)
        {
            this.combatSlotRegistry = combatSlotRegistry;
            this.contextProvider = contextProvider;
            this.cardExecutor = cardExecutor;
            this.enemyHandManager = enemyHandManager;
        }

        public void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor)
        {
            contextProvider = provider;
            cardExecutor = executor;
        }

        public void SetTurnController(ITurnStateController controller)
        {
            turnController = controller;
        }

        public IEnumerator PerformAttack(CombatSlotPosition slotPosition)
        {
            var fieldSlot = SlotPositionUtil.ToFieldSlot(slotPosition);
            var slot = combatSlotRegistry.GetCombatSlot(fieldSlot);

            if (slot == null || slot.IsEmpty())
            {
                Debug.LogWarning($"[Executor] 슬롯 {slotPosition}에 카드가 없습니다.");
                yield break;
            }

            var card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning($"[Executor] 슬롯 {slotPosition}에 등록된 카드가 null입니다.");
                yield break;
            }

            Debug.Log($"[Executor] 카드 실행 시작: {card.GetCardName()}");

            var context = contextProvider.CreateContext(card);
            cardExecutor.Execute(card, context, turnController);

            yield return new WaitForSeconds(0.5f); // 추후 IWaitService 등으로 분리 가능

            slot.Clear(); // 필요 시 ICombatSlotClearService로 추출 가능
            Debug.Log($"[Executor] 슬롯 {slotPosition} 클리어 완료");
        }
    }
}
