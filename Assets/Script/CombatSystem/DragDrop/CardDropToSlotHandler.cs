using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.DragDrop
{
    public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
    {
        private CardDropService dropService;
        private ICombatFlowCoordinator flowCoordinator;

        public void Inject(CardDropService dropService, ICombatFlowCoordinator coordinator)
        {
            this.dropService = dropService;
            this.flowCoordinator = coordinator;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!PlayerInputGuard.CanProceed(flowCoordinator)) return;

            var cardUI = eventData.pointerDrag?.GetComponent<SkillCardUI>();
            var card = cardUI?.GetCard();
            var dragHandler = eventData.pointerDrag?.GetComponent<CardDragHandler>();
            var slot = GetComponent<ICombatCardSlot>();

            if (cardUI == null || card == null || dragHandler == null || slot == null)
            {
                Debug.LogWarning("[DropHandler] 필수 요소가 누락됨");
                return;
            }

            if (dropService.TryDropCard(card, cardUI, slot, out var message))
            {
                dragHandler.droppedSuccessfully = true;

                // 드롭 성공 시 새로운 복귀 기준 지정
                var slotTransform = ((MonoBehaviour)slot).transform;
                dragHandler.OriginalParent = slotTransform;
                dragHandler.OriginalWorldPosition = slotTransform.position;

                Debug.Log($"[DropHandler] 드롭 성공: {card.CardData.Name}");
            }
            else
            {
                dragHandler.droppedSuccessfully = false;
                CardSlotHelper.ResetCardToOriginal(cardUI);
                Debug.LogWarning($"[DropHandler] 드롭 실패: {message}");
            }
        }
    }
}
