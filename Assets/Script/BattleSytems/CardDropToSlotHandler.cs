using UnityEngine;
using UnityEngine.EventSystems;
using Game.UI;
using Game.Interface;

namespace Game.Battle
{
    /// <summary>
    /// 플레이어가 카드를 드래그하여 전투 슬롯에 놓았을 때 처리합니다.
    /// </summary>
    public class BattleSlotDropHandler : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            var dragged = eventData.pointerDrag;
            if (dragged != null && dragged.TryGetComponent(out CardUI draggedCard))
            {
                if (TryGetComponent(out ICardSlot slot))
                {
                    slot.SetCard(draggedCard.cardData);
                    Destroy(dragged); // 드래그한 원본 제거
                }
            }
        }
    }
}
