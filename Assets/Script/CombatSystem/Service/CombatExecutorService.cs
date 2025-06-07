using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Executor
{
    /// <summary>
    /// 전투 실행 서비스. 슬롯에 배치된 카드를 실행하고 결과를 처리함.
    /// </summary>
    public class CombatExecutorService : ICombatExecutorService, ICombatExecutor
    {
        private readonly ICombatSlotRegistry combatSlotRegistry;
        private ICardExecutionContextProvider contextProvider;
        private ICardExecutor cardExecutor;
        private readonly IEnemyHandManager enemyHandManager;
        private ICombatTurnManager turnManager;

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

        /// <summary>
        /// CombatFlowCoordinator 또는 GameManager에서 호출하여 실행
        /// </summary>
        public IEnumerator ExecuteCombatPhase()
        {
            yield return PerformAttack(CombatSlotPosition.FIRST);
            yield return PerformAttack(CombatSlotPosition.SECOND);
        }

        /// <summary>
        /// 단일 슬롯 위치에 대해 카드 실행을 수행
        /// </summary>
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
            cardExecutor.Execute(card, context, turnManager);

            yield return new WaitForSeconds(0.5f);

            slot.ClearAll();
            Debug.Log($"[Executor] 슬롯 {slotPosition} 클리어 완료");
        }

        /// <summary>
        /// 동적으로 Execution 관련 종속 객체를 변경할 수 있음
        /// </summary>
        public void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor)
        {
            contextProvider = provider;
            cardExecutor = executor;
        }

        /// <summary>
        /// 전투 턴 매니저 주입 (기존 ITurnStateController → ICombatTurnManager로 변경됨)
        /// </summary>
        public void SetTurnManager(ICombatTurnManager manager)
        {
            turnManager = manager;
        }
    }
}
