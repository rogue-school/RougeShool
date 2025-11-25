using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Manager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Service;
using Zenject;
using Game.CoreSystem.Utility;
using Game.SkillCardSystem.DragDrop;

namespace Game.CombatSystem.DragDrop
{
    public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
    {
        private CardDropService _dropService;
        // CombatSlotManager 제거됨 - 슬롯 관리 기능을 CombatFlowManager로 통합

        [Inject]
        public void Construct(CardDropService dropService)
        {
            _dropService = dropService;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var cardUI = eventData.pointerDrag?.GetComponent<SkillCardUI>();
            var card = cardUI?.GetCard();
            var dragHandler = eventData.pointerDrag?.GetComponent<CardDragHandler>();
            // CombatSlotManager 제거됨 - 슬롯 검증을 다른 방식으로 처리
            // 전투 슬롯 또는 핸드 슬롯 인터페이스를 우선 조회
            Game.CombatSystem.Interface.ICombatCardSlot slot = GetComponent<Game.CombatSystem.Interface.ICombatCardSlot>();
            if (slot == null)
            {
                // 필요한 경우 다른 슬롯 타입(IHandCardSlot 등)으로 확장 가능
                // var handSlot = GetComponent<Game.CombatSystem.Interface.IHandCardSlot>();
            }

            if (slot == null)
            {
                GameLogger.LogWarning("[DropHandler] 슬롯이 null입니다.", GameLogger.LogCategory.SkillCard);
                dragHandler?.ResetToOrigin(cardUI);
                return;
            }

            if (_dropService == null)
            {
                GameLogger.LogError("[DropHandler] ❗ dropService가 null입니다. Zenject 설정 확인 필요.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (cardUI == null || card == null || dragHandler == null)
            {
                GameLogger.LogWarning("[DropHandler] 필수 드롭 요소 누락됨", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (_dropService.TryDropCard(card, cardUI, slot, out var message))
            {
                dragHandler.droppedSuccessfully = true;

                // 슬롯 Transform으로 스냅/정렬은 DropService 내부에서 처리되도록 유지

                // 드롭 성공
            }
            else
            {
                dragHandler.droppedSuccessfully = false;
                dragHandler.ResetToOrigin(cardUI);
                GameLogger.LogWarning($"[DropHandler] 드롭 실패: {message}", GameLogger.LogCategory.SkillCard);
            }
        }
    }
}
