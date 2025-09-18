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
        // 적 핸드 시스템 제거
        private ICombatTurnManager turnManager;

        /// <summary>
        /// 생성자 - 전투 슬롯, 컨텍스트 제공자, 카드 실행기를 주입받습니다.
        /// </summary>
        public CombatExecutorService(
            ICombatSlotRegistry combatSlotRegistry,
            ICardExecutionContextProvider contextProvider,
            ICardExecutor cardExecutor)
        {
            this.combatSlotRegistry = combatSlotRegistry;
            this.contextProvider = contextProvider;
            this.cardExecutor = cardExecutor;
        }

        #endregion

        #region 전투 실행

        /// <summary>
        /// 1번 슬롯에서만 카드를 실행합니다. (레거시 시스템용)
        /// 카드 사용 후 슬롯을 한 칸씩 이동시킵니다.
        /// </summary>
        public IEnumerator ExecuteCardInSlot1()
        {
#pragma warning disable CS0618 // 레거시 호환성을 위해 의도적으로 사용
            yield return PerformAttack(CombatSlotPosition.SLOT_1);
#pragma warning restore CS0618
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 전투슬롯에서 카드를 실행합니다.
        /// </summary>
        public IEnumerator ExecuteCardInBattleSlot()
        {
            yield return PerformAttackNew(CombatSlotPosition.BATTLE_SLOT);
        }

        /// <summary>
        /// 즉시 실행 모드: 1번 슬롯에 카드 배치 시 즉시 실행 (레거시 시스템용)
        /// </summary>
        public void ExecuteImmediately()
        {
#pragma warning disable CS0618 // 레거시 호환성을 위해 의도적으로 사용
            var slot = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.SLOT_1);
#pragma warning restore CS0618

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

        // 최적화: 정적 배열로 메모리 할당 방지 (레거시 시스템용)
#pragma warning disable CS0618 // 레거시 호환성을 위해 의도적으로 사용
        private static readonly CombatSlotPosition[] SlotMoveOrder = 
        {
            CombatSlotPosition.SLOT_4,
            CombatSlotPosition.SLOT_3,
            CombatSlotPosition.SLOT_2,
            CombatSlotPosition.SLOT_1
        };
#pragma warning restore CS0618

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
                slots[i] = combatSlotRegistry.GetCombatSlot(SlotMoveOrder[i]);
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
        /// 지정한 슬롯 위치에 있는 카드의 효과를 실행합니다. (레거시 시스템용)
        /// 1번 슬롯에서만 실행되며, 실행 후 슬롯을 이동시킵니다.
        /// </summary>
        /// <param name="slotPosition">실행할 슬롯 위치 (SLOT_1만 지원)</param>
        public IEnumerator PerformAttack(CombatSlotPosition slotPosition)
        {
            // 1번 슬롯에서만 실행 가능
#pragma warning disable CS0618 // 레거시 호환성을 위해 의도적으로 사용
            if (slotPosition != CombatSlotPosition.SLOT_1)
#pragma warning restore CS0618
            {
                Debug.LogWarning($"[Executor] 1번 슬롯에서만 카드 실행 가능합니다. 요청된 슬롯: {slotPosition}");
                yield break;
            }

            var slot = combatSlotRegistry.GetCombatSlot(slotPosition);

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

        #region 새로운 5슬롯 시스템

        /// <summary>
        /// 새로운 5슬롯 시스템: 전투슬롯에서 카드를 즉시 실행합니다.
        /// </summary>
        public void ExecuteImmediatelyNew()
        {
            var slot = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.BATTLE_SLOT);

            if (slot == null || slot.IsEmpty())
            {
                Debug.LogWarning("[Executor] 전투슬롯에 카드가 없습니다.");
                return;
            }

            var card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning("[Executor] 전투슬롯에 등록된 카드가 null입니다.");
                return;
            }

            var context = contextProvider.CreateContext(card);
            cardExecutor.Execute(card, context, turnManager);

            // 카드 사용 후 슬롯 이동
            MoveSlotsForwardNew();
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 지정한 슬롯 위치에 있는 카드의 효과를 실행합니다.
        /// 전투슬롯에서만 실행되며, 실행 후 슬롯을 이동시킵니다.
        /// </summary>
        /// <param name="slotPosition">실행할 슬롯 위치 (BATTLE_SLOT만 지원)</param>
        public IEnumerator PerformAttackNew(CombatSlotPosition slotPosition)
        {
            // 전투슬롯에서만 실행 가능
            if (slotPosition != CombatSlotPosition.BATTLE_SLOT)
            {
                Debug.LogWarning($"[Executor] 전투슬롯에서만 카드 실행 가능합니다. 요청된 슬롯: {slotPosition}");
                yield break;
            }

            var slot = combatSlotRegistry.GetCombatSlot(slotPosition);

            if (slot == null || slot.IsEmpty())
            {
                Debug.LogWarning("[Executor] 전투슬롯에 카드가 없습니다.");
                yield break;
            }

            var card = slot.GetCard();
            if (card == null)
            {
                Debug.LogWarning("[Executor] 전투슬롯에 등록된 카드가 null입니다.");
                yield break;
            }

            Debug.Log($"[Executor] 전투슬롯에서 카드 실행: {card.GetCardName()}");

            var context = contextProvider.CreateContext(card);
            cardExecutor.Execute(card, context, turnManager);

            // 카드 사용 후 슬롯 이동
            MoveSlotsForwardNew();

            yield return null;
        }

        // 새로운 5슬롯 시스템용 정적 배열
        private static readonly CombatSlotPosition[] NewSlotMoveOrder = 
        {
            CombatSlotPosition.WAIT_SLOT_4,
            CombatSlotPosition.WAIT_SLOT_3,
            CombatSlotPosition.WAIT_SLOT_2,
            CombatSlotPosition.WAIT_SLOT_1,
            CombatSlotPosition.BATTLE_SLOT
        };

        /// <summary>
        /// 새로운 5슬롯 시스템: 슬롯을 한 칸씩 앞으로 이동시킵니다.
        /// 대기4→대기3→대기2→대기1→전투슬롯으로 이동
        /// </summary>
        private void MoveSlotsForwardNew()
        {
            // 슬롯 참조 캐싱 (스택 할당으로 GC 압박 최소화)
            var slots = new ICombatCardSlot[5];
            
            // 슬롯 조회 및 검증
            for (int i = 0; i < 5; i++)
            {
                slots[i] = combatSlotRegistry.GetCombatSlot(NewSlotMoveOrder[i]);
                if (slots[i] == null)
                {
                    Debug.LogWarning($"[Executor] 슬롯 {NewSlotMoveOrder[i]}을 찾을 수 없습니다.");
                    return;
                }
            }

            // 역순으로 이동하여 데이터 손실 방지
            for (int i = 0; i < 4; i++)
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

            Debug.Log("[Executor] 새로운 5슬롯 시스템 슬롯 이동 완료: 대기4→대기3→대기2→대기1→전투슬롯");
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 셋업 단계에서 카드를 배치합니다.
        /// </summary>
        /// <param name="slotPosition">배치할 슬롯 위치</param>
        /// <param name="card">배치할 카드</param>
        public void PlaceCardInSetup(CombatSlotPosition slotPosition, ISkillCard card)
        {
            if (slotPosition == CombatSlotPosition.NONE)
            {
                Debug.LogWarning("[Executor] 유효하지 않은 슬롯 위치입니다.");
                return;
            }

            var slot = combatSlotRegistry.GetCombatSlot(slotPosition);
            if (slot == null)
            {
                Debug.LogWarning($"[Executor] 슬롯 {slotPosition}을 찾을 수 없습니다.");
                return;
            }

            slot.SetCard(card);
            Debug.Log($"[Executor] 셋업 단계에서 {slotPosition}에 카드 배치: {card.GetCardName()}");
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 전투슬롯에 카드가 있는지 확인합니다.
        /// </summary>
        /// <returns>전투슬롯에 카드가 있으면 true</returns>
        public bool HasCardInBattleSlot()
        {
            var slot = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.BATTLE_SLOT);
            return slot != null && !slot.IsEmpty();
        }

        /// <summary>
        /// 새로운 5슬롯 시스템: 전투슬롯의 카드를 반환합니다.
        /// </summary>
        /// <returns>전투슬롯의 카드, 없으면 null</returns>
        public ISkillCard GetCardInBattleSlot()
        {
            var slot = combatSlotRegistry.GetCombatSlot(CombatSlotPosition.BATTLE_SLOT);
            return slot?.GetCard();
        }

        #endregion
    }
}
