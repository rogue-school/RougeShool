using Game.SkillCardSystem.UI;
using UnityEngine;
using Game.CombatSystem.DragDrop;

namespace Game.CombatSystem.Utility
{
    public static class CardSlotHelper
    {
        public static void ResetCardToOriginal(SkillCardUI cardUI)
        {
            var dragHandler = cardUI?.GetComponent<CardDragHandler>();
            if (dragHandler == null)
            {
                Debug.LogWarning("[Helper] DragHandler 없음: " + cardUI?.name);
                return;
            }

            Debug.Log($"[Helper] 카드 원위치 복귀: {cardUI.name}, 위치: {dragHandler.OriginalWorldPosition}, 부모: {dragHandler.OriginalParent?.name}");

            cardUI.transform.SetParent(dragHandler.OriginalParent, false);
            cardUI.transform.position = dragHandler.OriginalWorldPosition;
            cardUI.transform.localScale = Vector3.one;

            // 다시 드래그 시 새로운 위치 저장을 위해 초기화
            dragHandler.OriginalParent = null;
        }

        public static void AttachCardToSlot(SkillCardUI cardUI, MonoBehaviour slotTransform)
        {
            var rect = cardUI.GetComponent<RectTransform>();
            rect.SetParent(slotTransform.transform, false);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            var dragHandler = cardUI.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.OriginalParent = null; // 이후 드래그 시 새 위치 저장 가능
            }

            Debug.Log($"[Helper] 카드 슬롯에 정렬: {cardUI.name} → {slotTransform.name}");
        }
    }
}
