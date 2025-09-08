using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Service;
using Zenject;

namespace Game.CombatSystem.DragDrop
{
    public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
    {
        private CardDropService _dropService;
        private ICombatFlowCoordinator _flowCoordinator;

        [Inject]
        public void Construct(CardDropService dropService, ICombatFlowCoordinator flowCoordinator)
        {
            _dropService = dropService;
            _flowCoordinator = flowCoordinator;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var cardUI = eventData.pointerDrag?.GetComponent<SkillCardUI>();
            var card = cardUI?.GetCard();
            var dragHandler = eventData.pointerDrag?.GetComponent<CardDragHandler>();
            var slot = GetComponent<ICombatCardSlot>();

            if (slot == null)
            {
                Debug.LogWarning("[DropHandler] 슬롯이 null입니다.");
                dragHandler?.ResetToOrigin(cardUI);
                return;
            }

            if (_dropService == null)
            {
                Debug.LogError("[DropHandler] ❗ dropService가 null입니다. Zenject 설정 확인 필요.");
                return;
            }

            if (cardUI == null || card == null || dragHandler == null)
            {
                Debug.LogWarning("[DropHandler] 필수 드롭 요소 누락됨");
                return;
            }

            if (_dropService.TryDropCard(card, cardUI, slot, out var message))
            {
                dragHandler.droppedSuccessfully = true;

                if (slot is MonoBehaviour slotMb)
                {
                    dragHandler.OriginalParent = slotMb.transform;
                    dragHandler.OriginalWorldPosition = slotMb.transform.position;
                }

                // 드롭 성공
            }
            else
            {
                dragHandler.droppedSuccessfully = false;
                dragHandler.ResetToOrigin(cardUI);
                Debug.LogWarning($"[DropHandler] 드롭 실패: {message}");
            }
        }
    }
}
