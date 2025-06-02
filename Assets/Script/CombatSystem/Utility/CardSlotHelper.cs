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

            //  1. 부모 설정
            cardUI.transform.SetParent(dragHandler.OriginalParent, false);

            //  2. localPosition 기준으로 정확하게 중심으로 맞춤
            if (cardUI.TryGetComponent(out RectTransform rectTransform))
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
            }
            else
            {
                cardUI.transform.localPosition = Vector3.zero;
                cardUI.transform.localRotation = Quaternion.identity;
                cardUI.transform.localScale = Vector3.one;
            }

            //Debug.Log($"[CardSlotHelper] 카드 복귀 완료: {cardUI.name}, parent: {dragHandler.OriginalParent.name}");
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
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;

            // dragHandler.OriginalParent = null 제거함
            Debug.Log($"[CardSlotHelper] 카드 배치 완료: {cardUI.name} → {slotTransform.name}");
        }
    }
}
