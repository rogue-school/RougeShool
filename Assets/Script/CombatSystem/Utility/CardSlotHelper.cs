using UnityEngine;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.DragDrop;

namespace Game.CombatSystem.Utility
{
    public static class CardSlotHelper
    {
        public static void ResetCardToOriginal(SkillCardUI cardUI)
        {
            if (cardUI == null)
            {
                Debug.LogWarning("[CardSlotHelper] cardUI가 null입니다.");
                return;
            }

            var dragHandler = cardUI.GetComponent<CardDragHandler>();
            if (dragHandler == null || dragHandler.OriginalParent == null)
            {
                Debug.LogWarning($"[CardSlotHelper] 복귀 실패: dragHandler 또는 원래 부모가 null - {cardUI.name}");
                return;
            }

            cardUI.transform.SetParent(dragHandler.OriginalParent, false);
            cardUI.transform.position = dragHandler.OriginalWorldPosition;
            cardUI.transform.localScale = Vector3.one;

            dragHandler.OriginalParent = null;

            Debug.Log($"[CardSlotHelper] 카드 복귀 완료: {cardUI.name}");
        }

        public static void AttachCardToSlot(SkillCardUI cardUI, MonoBehaviour slotTransform)
        {
            if (cardUI == null || slotTransform == null)
            {
                Debug.LogWarning("[CardSlotHelper] 카드 UI 또는 슬롯이 null입니다.");
                return;
            }

            var rect = cardUI.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogWarning($"[CardSlotHelper] RectTransform이 없습니다 - {cardUI.name}");
                return;
            }

            rect.SetParent(slotTransform.transform, false);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            var dragHandler = cardUI.GetComponent<CardDragHandler>();
            if (dragHandler != null)
                dragHandler.OriginalParent = null;

            Debug.Log($"[CardSlotHelper] 카드 배치 완료: {cardUI.name} → {slotTransform.name}");
        }
    }
}
