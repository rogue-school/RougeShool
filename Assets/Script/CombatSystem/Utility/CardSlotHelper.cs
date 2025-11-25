using UnityEngine;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.DragDrop;
using Game.SkillCardSystem.Slot;
using System.Linq;
using Game.CombatSystem.UI;

namespace Game.CombatSystem.Utility
{
    public static class CardSlotHelper
    {
        private static readonly Vector2 kCardAnchoredOffset = new Vector2(0f, 4f);
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

            // 부모 변경
            cardUI.transform.SetParent(dragHandler.OriginalParent, false);
            
            // 원래 형제 순서로 복원 (리플렉션 사용)
            var siblingIndexField = typeof(CardDragHandler).GetField("originalSiblingIndex", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (siblingIndexField != null)
            {
                var originalIndex = (int)siblingIndexField.GetValue(dragHandler);
                cardUI.transform.SetSiblingIndex(originalIndex);
            }

            if (cardUI.TryGetComponent(out RectTransform rectTransform))
            {
                rectTransform.anchoredPosition = kCardAnchoredOffset;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
            }
            else
            {
                cardUI.transform.localPosition = Vector3.zero;
                cardUI.transform.localRotation = Quaternion.identity;
                cardUI.transform.localScale = Vector3.one;
            }
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
            rect.localPosition = Vector3.zero;
            rect.anchoredPosition = kCardAnchoredOffset;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
        public static void AttachCardToHandSlot(SkillCardUI cardUI, SkillCardSlotPosition slotPos)
        {
            var handSlots = Object.FindObjectsByType<PlayerHandCardSlotUI>(FindObjectsSortMode.None);
            var targetSlot = handSlots.FirstOrDefault(s => s.GetSlotPosition() == slotPos);

            if (targetSlot != null)
            {
                cardUI.transform.SetParent(targetSlot.transform, false);
                cardUI.transform.position = targetSlot.transform.position;
            }
            else
            {
                Debug.LogWarning($"[CardSlotHelper] 해당 핸드 슬롯을 찾을 수 없습니다: {slotPos}");
            }
        }
    }
}
