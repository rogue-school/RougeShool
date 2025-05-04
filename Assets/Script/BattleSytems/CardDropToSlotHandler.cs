using UnityEngine;
using Game.Interface;
using Game.Events;

namespace Game.Battle
{
    /// <summary>
    /// 드래그된 카드를 슬롯에 드롭했을 때, 해당 슬롯에 할당하는 역할을 수행합니다.
    /// </summary>
    public class CardDropToSlotHandler : MonoBehaviour
    {
        private ICardSlot slot;

        private void Awake()
        {
            slot = GetComponent<ICardSlot>();
            if (slot == null)
                Debug.LogError("[CardDropToSlotHandler] ICardSlot 컴포넌트를 찾을 수 없습니다.");
        }

        private void OnMouseUpAsButton()
        {
            ISkillCard draggedCard = CardDragHandler.CurrentCard;
            if (draggedCard == null || slot == null) return;

            // 슬롯에 카드 할당
            slot.SetCard(draggedCard);

            // 모든 리스너에게 카드 드롭 알림 전송
            CardDropEventSystem.NotifyCardDropped(draggedCard, slot);

            // 드래그 해제
            CardDragHandler.Clear();
        }
    }
}
