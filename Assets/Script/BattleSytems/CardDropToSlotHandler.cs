using Game.Cards;
using Game.Interface;
using Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어 카드가 전투 슬롯에 드롭되었을 때 처리하는 핸들러입니다.
/// 적 카드가 존재하면 드롭을 막고, 기존 플레이어 카드가 있다면 교체합니다.
/// </summary>
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

        // 적 카드가 이미 있으면 드롭 금지
        if (oldCard != null && !IsPlayerCard(oldCard))
        {
            Debug.LogWarning("[DropHandler] 해당 슬롯에 적 카드가 있어 드롭 불가");
            return;
        }

        // 기존 플레이어 카드 복귀
        if (oldCardUI != null)
        {
            var oldDragHandler = oldCardUI.GetComponent<CardDragHandler>();
            if (oldDragHandler != null)
            {
                oldCardUI.transform.SetParent(oldDragHandler.OriginalParent);
                oldCardUI.transform.localPosition = oldDragHandler.OriginalPosition;
                oldCardUI.transform.localScale = Vector3.one;
            }

            slot.Clear(); // 슬롯 참조만 초기화 (파괴는 하지 않음)
        }

        // 새 카드 등록
        newCard.SetCombatSlot(slot.GetCombatPosition());
        slot.SetCard(newCard);
        slot.SetCardUI(newCardUI);

        // UI를 슬롯에 배치
        draggedObject.transform.SetParent(((MonoBehaviour)slot).transform);
        draggedObject.transform.localPosition = Vector3.zero;
        draggedObject.transform.localScale = Vector3.one;

        // 드롭 성공 처리
        var dragHandler = draggedObject.GetComponent<CardDragHandler>();
        if (dragHandler != null)
            dragHandler.droppedSuccessfully = true;

        Debug.Log($"[DropHandler] 카드 교체 완료: {newCard.GetCardName()} → {slot.GetCombatPosition()}");
    }

    private bool IsPlayerCard(ISkillCard card)
    {
        var handSlot = card.GetHandSlot();
        return handSlot.HasValue && handSlot.Value.ToString().Contains("PLAYER");
    }
}
