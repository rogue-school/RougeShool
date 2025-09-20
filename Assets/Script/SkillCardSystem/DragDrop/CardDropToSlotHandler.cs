using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Service;
using Zenject;

namespace Game.CombatSystem.DragDrop
{
    public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
    {
        private CardDropService _dropService;
        private CombatSlotManager _slotManager;

        [Inject]
        public void Construct(CardDropService dropService, CombatSlotManager slotManager)
        {
            _dropService = dropService;
            _slotManager = slotManager;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var cardUI = eventData.pointerDrag?.GetComponent<SkillCardUI>();
            var card = cardUI?.GetCard();
            var dragHandler = eventData.pointerDrag?.GetComponent<CardDragHandler>();
            var slot = GetComponent<CombatSlot>();

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

                // CombatSlot은 MonoBehaviour가 아니므로 transform 접근 불가
                // 대신 슬롯의 위치 정보를 다른 방식으로 처리해야 함
                // TODO: 슬롯 위치 정보를 위한 대안 구현 필요

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
