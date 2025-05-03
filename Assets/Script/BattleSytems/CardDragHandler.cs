using UnityEngine;
using UnityEngine.EventSystems;
using Game.Interface;

namespace Game.Battle
{
    /// <summary>
    /// 드래그한 카드를 전투 슬롯에 드롭했을 때 처리하는 핸들러입니다.
    /// </summary>
    public class CardDropToSlotHandler : MonoBehaviour, IDropHandler
    {
        [SerializeField] private MonoBehaviour targetSlot;

        public void OnDrop(PointerEventData eventData)
        {
            if (targetSlot is ICardSlot cardSlot && eventData.pointerDrag != null)
            {
                var dragHandler = eventData.pointerDrag.GetComponent<CardDragHandler>();
                if (dragHandler != null && dragHandler.HasCard())
                {
                    cardSlot.SetCard(dragHandler.GetCard());
                }
            }
        }
    }
}
