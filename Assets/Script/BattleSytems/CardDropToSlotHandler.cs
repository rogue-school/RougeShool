using UnityEngine;
using UnityEngine.EventSystems;
using Game.Interface;
using Game.Slots;
using Game.UI;

namespace Game.Utility
{
    /// <summary>
    /// 카드가 드롭되었을 때의 처리를 담당합니다.
    /// 슬롯 오브젝트에 부착되어야 하며, ICombatCardSlot을 구현한 슬롯이어야 합니다.
    /// </summary>
    public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            // 드래그된 카드가 있는지 확인
            GameObject draggedObject = eventData.pointerDrag;
            if (draggedObject == null) return;

            // 드래그된 카드가 ISkillCardUI를 포함하고 있는지 확인
            SkillCardUI cardUI = draggedObject.GetComponent<SkillCardUI>();
            if (cardUI == null) return;

            ISkillCard card = cardUI.GetCard();
            if (card == null) return;

            // 드롭된 슬롯이 ICombatCardSlot인지 확인
            var slot = GetComponent<ICombatCardSlot>();
            if (slot == null) return;

            // 카드에 슬롯 위치를 주입하고 슬롯에 등록
            var combatPos = slot.GetCombatPosition();
            card.SetCombatSlot(combatPos);
            slot.SetCard(card);

            // 카드 UI를 슬롯에 붙임
            draggedObject.transform.SetParent(((MonoBehaviour)slot).transform);
            draggedObject.transform.localPosition = Vector3.zero;

            Debug.Log($"[CardDropToSlotHandler] 카드 드롭 성공: {card.GetCardName()} → {combatPos}");
        }
    }
}
