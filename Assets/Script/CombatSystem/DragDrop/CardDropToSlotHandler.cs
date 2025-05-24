using UnityEngine;
using UnityEngine.EventSystems;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.DragDrop
{
    public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
    {
        private ITurnCardRegistry cardRegistry;

        public void Inject(ITurnCardRegistry registry)
        {
            this.cardRegistry = registry;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var draggedObject = eventData?.pointerDrag;
            if (draggedObject == null)
            {
                Debug.LogWarning("[DropHandler] 드래그된 객체가 null입니다.");
                return;
            }

            if (!draggedObject.TryGetComponent(out SkillCardUI newCardUI))
            {
                Debug.LogWarning("[DropHandler] SkillCardUI 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            var newCard = newCardUI.GetCard();
            if (newCard == null)
            {
                Debug.LogWarning("[DropHandler] SkillCard가 null입니다.");
                return;
            }

            if (!TryGetComponent(out ICombatCardSlot slot))
            {
                Debug.LogWarning("[DropHandler] 드롭 대상이 ICombatCardSlot이 아님. 드롭 실패 처리.");
                if (draggedObject.TryGetComponent(out CardDragHandler handler))
                {
                    handler.droppedSuccessfully = false;
                }
                return;
            }

            if (!draggedObject.TryGetComponent(out CardDragHandler dragHandler))
            {
                Debug.LogWarning("[DropHandler] 드래그 핸들러(CardDragHandler)를 찾을 수 없습니다.");
                return;
            }

            var slotOwner = slot.GetOwner();
            var combatPosition = slot.GetCombatPosition();

            Debug.Log($"[DropHandler] 드롭 시도: 카드 = {newCard.GetCardName()}, 슬롯 = {combatPosition}, 슬롯 소유자 = {slotOwner}");

            // 슬롯이 플레이어 것이 아닌 경우 드롭 불가
            if (slotOwner != SlotOwner.PLAYER)
            {
                dragHandler.droppedSuccessfully = false;
                Debug.LogWarning("[DropHandler] 플레이어 슬롯이 아님. 드롭 거부.");
                return;
            }

            // 카드가 플레이어 카드가 아닌 경우 드롭 불가
            if (!CardValidator.IsPlayerCard(newCard))
            {
                dragHandler.droppedSuccessfully = false;
                Debug.LogWarning("[DropHandler] 플레이어 카드가 아님. 드롭 거부.");
                return;
            }

            var oldCard = slot.GetCard();
            var oldCardUI = slot.GetCardUI();

            // 기존에 적 카드가 올라와 있으면 드롭 불가
            if (oldCard != null && !CardValidator.IsPlayerCard(oldCard))
            {
                dragHandler.droppedSuccessfully = false;
                Debug.LogWarning("[DropHandler] 적 카드 위에 드롭은 불가. 드롭 거부.");
                return;
            }

            // 기존 카드가 있으면 되돌리기
            if (oldCardUI != null)
            {
                Debug.Log("[DropHandler] 기존 카드가 있어 원래 위치로 복귀 처리.");
                CardSlotHelper.ResetCardToOriginal(oldCardUI);
                cardRegistry.ClearSlot(combatPosition);
            }

            // 슬롯에 카드 등록 및 UI 연결
            CardRegistrar.RegisterCard(slot, newCard, newCardUI);
            CardSlotHelper.AttachCardToSlot(newCardUI, (MonoBehaviour)slot);
            cardRegistry.RegisterPlayerCard(combatPosition, newCard);

            dragHandler.droppedSuccessfully = true;
            Debug.Log($"[DropHandler] 드롭 성공! 카드: {newCard.GetCardName()} → 슬롯: {combatPosition}");
        }
    }
}
