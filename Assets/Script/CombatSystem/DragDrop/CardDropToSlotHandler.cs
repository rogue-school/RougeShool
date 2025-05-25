using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Core;

namespace Game.CombatSystem.DragDrop
{
    /// <summary>
    /// 카드가 전투 슬롯 위에 드롭될 때 처리하는 핸들러
    /// </summary>
    public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
    {
        private ITurnCardRegistry cardRegistry;
        private ICombatFlowCoordinator flowCoordinator;
        private ICombatTurnManager combatTurnManager;

        /// <summary>
        /// 기본 카드 등록 시스템 주입
        /// </summary>
        public void Inject(ITurnCardRegistry registry)
        {
            this.cardRegistry = registry;
        }

        /// <summary>
        /// 의존성 일괄 주입 (BootstrapInstaller용)
        /// </summary>
        public void InjectDependencies(ITurnCardRegistry registry, ICombatFlowCoordinator coordinator, ICombatTurnManager turnManager)
        {
            this.cardRegistry = registry;
            this.flowCoordinator = coordinator;
            this.combatTurnManager = turnManager;
        }

        private void Awake()
        {
            // 예외적으로 수동 주입이 없으면 기본 FlowCoordinator 검색
            if (flowCoordinator == null)
            {
                flowCoordinator = Object.FindFirstObjectByType<CombatFlowCoordinator>();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (flowCoordinator == null || !flowCoordinator.IsPlayerInputEnabled())
            {
                Debug.LogWarning("[DropHandler] 드롭 거부: 플레이어 입력이 비활성화됨");
                return;
            }

            if (eventData?.pointerDrag == null)
            {
                Debug.LogWarning("[DropHandler] 드래그된 오브젝트가 null입니다.");
                return;
            }

            if (!eventData.pointerDrag.TryGetComponent(out SkillCardUI newCardUI) || newCardUI.GetCard() == null)
            {
                Debug.LogWarning("[DropHandler] SkillCardUI 또는 카드 데이터가 없습니다.");
                return;
            }

            var newCard = newCardUI.GetCard();

            if (!eventData.pointerDrag.TryGetComponent(out CardDragHandler dragHandler))
            {
                Debug.LogError("[DropHandler] 드래그 핸들러(CardDragHandler)가 없습니다.");
                return;
            }

            if (!TryGetComponent(out ICombatCardSlot slot))
            {
                Debug.LogError("[DropHandler] 드롭 대상에 ICombatCardSlot이 없습니다.");
                dragHandler.droppedSuccessfully = false;
                return;
            }

            if (cardRegistry == null)
            {
                Debug.LogError("[DropHandler] cardRegistry가 주입되지 않았습니다.");
                dragHandler.droppedSuccessfully = false;
                return;
            }

            if (!newCard.IsFromPlayer())
            {
                Debug.LogWarning("[DropHandler] 플레이어 카드가 아닌 경우 드롭 불가");
                dragHandler.droppedSuccessfully = false;
                return;
            }

            var existingCard = slot.GetCard();
            var existingCardUI = slot.GetCardUI();

            // 적 카드 덮어쓰기 방지
            if (existingCard != null && !existingCard.IsFromPlayer())
            {
                Debug.LogWarning("[DropHandler] 적 카드가 슬롯에 존재. 덮어쓰기 금지");
                dragHandler.droppedSuccessfully = false;
                return;
            }

            // 기존 카드가 있다면 복귀 및 슬롯 초기화
            if (existingCardUI != null)
            {
                CardSlotHelper.ResetCardToOriginal(existingCardUI);
                cardRegistry.ClearSlot(SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition()));
            }

            // 카드 등록 및 배치
            CardRegistrar.RegisterCard(slot, newCard, newCardUI);
            CardSlotHelper.AttachCardToSlot(newCardUI, (MonoBehaviour)slot);
            cardRegistry.RegisterPlayerCard(SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition()), newCard);

            // CombatTurnManager에 등록 (전투 시작 조건 판단용)
            combatTurnManager?.RegisterPlayerCard(SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition()), newCard);

            dragHandler.droppedSuccessfully = true;
            Debug.Log($"[DropHandler] 드롭 성공: 카드 '{newCard.CardData.Name}' 이 슬롯 {slot.GetCombatPosition()} 에 등록됨");
        }
    }
}

