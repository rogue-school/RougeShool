using Game.Cards;
using Game.Interface;
using Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        SkillCardUI newCardUI = draggedObject.GetComponent<SkillCardUI>();
        if (newCardUI == null) return;

        ISkillCard newCard = newCardUI.GetCard();
        if (newCard == null) return;

        var slot = GetComponent<ICombatCardSlot>();
        if (slot == null) return;

        var oldCard = slot.GetCard();
        var oldCardUI = slot.GetCardUI();

        // 적 카드가 이미 있는 경우 드롭 실패
        if (oldCard != null && !(oldCard is RuntimeSkillCard))
        {
            Debug.LogWarning("[DropHandler] 슬롯에 적 카드가 있어 드롭 실패");
            return;
        }

        // 기존 플레이어 카드가 있으면 원래 위치로 되돌림
        if (oldCardUI != null)
        {
            var oldDragHandler = oldCardUI.GetComponent<CardDragHandler>();
            if (oldDragHandler != null)
            {
                oldCardUI.transform.SetParent(oldDragHandler.OriginalParent);
                oldCardUI.transform.localPosition = oldDragHandler.OriginalPosition;
                oldCardUI.transform.localScale = Vector3.one;
            }

            slot.Clear(); //  슬롯에서 기존 카드 정보 제거
        }

        // 새 카드 등록
        newCard.SetCombatSlot(slot.GetCombatPosition());
        slot.SetCard(newCard);
        slot.SetCardUI(newCardUI);

        // UI 이동
        draggedObject.transform.SetParent(((MonoBehaviour)slot).transform);
        draggedObject.transform.localPosition = Vector3.zero;
        draggedObject.transform.localScale = Vector3.one;

        // 드롭 성공 표시
        var dragHandler = draggedObject.GetComponent<CardDragHandler>();
        if (dragHandler != null)
            dragHandler.droppedSuccessfully = true;

        Debug.Log($"[DropHandler] 카드 교체 완료: {newCard.GetCardName()} → {slot.GetCombatPosition()}");
    }
}
