using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 전투 실행 서비스.
    /// 슬롯에 배치된 카드를 순차적으로 실행하고 결과를 처리합니다.
    /// </summary>
    public class CombatExecutorService : ICombatExecutorService, ICombatExecutor
    {
        #region 필드 및 생성자

        private readonly ICombatSlotRegistry combatSlotRegistry;
        private ICardExecutionContextProvider contextProvider;
        private ICardExecutor cardExecutor;
        private readonly IEnemyHandManager enemyHandManager;
        private ICombatTurnManager turnManager;

        /// <summary>
        /// 생성자 - 전투 슬롯, 컨텍스트 제공자, 카드 실행기, 적 핸드 매니저를 주입받습니다.
        /// </summary>
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

        #endregion

        #region 전투 실행

        /// <summary>
        /// 1번 슬롯에서만 카드를 실행합니다.
        /// 카드 사용 후 슬롯을 한 칸씩 이동시킵니다.
        /// </summary>
        public IEnumerator ExecuteCardInSlot1()
        {
            yield return PerformAttack(CombatSlotPosition.SLOT_1);
        }

        /// <summary>
        /// 즉시 실행 모드: 1번 슬롯에 카드 배치 시 즉시 실행
        /// </summary>
        public void ExecuteImmediately()
        {
            var fieldSlot = SlotPositionUtil.ToFieldSlot(CombatSlotPosition.SLOT_1);
            var slot = combatSlotRegistry.GetCombatSlot(fieldSlot);

            if (slot == null || slot.IsEmpty())
            {
                Debug.LogWarning("[Executor] 1번 슬롯에 카드가 없습니다.");
                return;
            }

            var card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning("[Executor] 1번 슬롯에 등록된 카드가 null입니다.");
                return;
            }

            var context = contextProvider.CreateContext(card);
            cardExecutor.Execute(card, context, turnManager);

            // 카드 사용 후 슬롯 이동
            MoveSlotsForward();
        }

        // 최적화: 정적 배열로 메모리 할당 방지
        private static readonly CombatSlotPosition[] SlotMoveOrder = 
        {
            CombatSlotPosition.SLOT_4,
            CombatSlotPosition.SLOT_3,
            CombatSlotPosition.SLOT_2,
            CombatSlotPosition.SLOT_1
        };

        /// <summary>
        /// 슬롯을 한 칸씩 앞으로 이동시킵니다.
        /// 2→1, 3→2, 4→3으로 이동 (최적화된 버전)
        /// </summary>
        private void MoveSlotsForward()
        {
            // 슬롯 참조 캐싱 (스택 할당으로 GC 압박 최소화)
            var slots = new ICombatCardSlot[4];
            
            // 슬롯 조회 및 검증
            for (int i = 0; i < 4; i++)
            {
                slots[i] = combatSlotRegistry.GetCombatSlot(SlotPositionUtil.ToFieldSlot(SlotMoveOrder[i]));
                if (slots[i] == null)
                {
                    Debug.LogWarning($"[Executor] 슬롯 {SlotMoveOrder[i]}을 찾을 수 없습니다.");
                    return;
                }
            }

            // 역순으로 이동하여 데이터 손실 방지
            for (int i = 0; i < 3; i++)
            {
                var sourceSlot = slots[i];
                var targetSlot = slots[i + 1];

                if (!sourceSlot.IsEmpty())
                {
                    var card = sourceSlot.GetCard();
                    targetSlot.SetCard(card);
                }
                sourceSlot.ClearAll();
            }

            Debug.Log("[Executor] 슬롯 이동 완료: 2→1, 3→2, 4→3");
        }

        /// <summary>
        /// 지정한 슬롯 위치에 있는 카드의 효과를 실행합니다.
        /// 1번 슬롯에서만 실행되며, 실행 후 슬롯을 이동시킵니다.
        /// </summary>
        /// <param name="slotPosition">실행할 슬롯 위치 (SLOT_1만 지원)</param>
        public IEnumerator PerformAttack(CombatSlotPosition slotPosition)
        {
            // 1번 슬롯에서만 실행 가능
            if (slotPosition != CombatSlotPosition.SLOT_1)
            {
                Debug.LogWarning($"[Executor] 1번 슬롯에서만 카드 실행 가능합니다. 요청된 슬롯: {slotPosition}");
                yield break;
            }

            var fieldSlot = SlotPositionUtil.ToFieldSlot(slotPosition);
            var slot = combatSlotRegistry.GetCombatSlot(fieldSlot);

            if (slot == null || slot.IsEmpty())
            {
                Debug.LogWarning("[Executor] 1번 슬롯에 카드가 없습니다.");
                yield break;
            }

            var card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning("[Executor] 1번 슬롯에 등록된 카드가 null입니다.");
                yield break;
            }

            var context = contextProvider.CreateContext(card);
            cardExecutor.Execute(card, context, turnManager);

            yield return new WaitForSeconds(0.5f);

            // 카드 사용 후 슬롯 이동
            MoveSlotsForward();
        }

        #endregion

        #region 종속성 주입

        /// <summary>
        /// 실행 관련 종속 객체(ICardExecutionContextProvider, ICardExecutor)를 동적으로 주입합니다.
        /// </summary>
        public void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor)
        {
            contextProvider = provider;
            cardExecutor = executor;
        }

        /// <summary>
        /// 전투 턴 매니저를 주입합니다.
        /// </summary>
        public void SetTurnManager(ICombatTurnManager manager)
        {
            turnManager = manager;
        }

        #endregion
    }
}
