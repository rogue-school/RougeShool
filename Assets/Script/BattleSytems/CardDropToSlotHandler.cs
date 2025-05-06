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
            {
                Debug.LogError("[CardDropToSlotHandler] ICardSlot 인터페이스를 구현한 컴포넌트를 찾을 수 없습니다.");
            }
        }

        private void OnMouseUpAsButton()
        {
            ISkillCard draggedCard = CardDragHandler.CurrentCard;

            if (draggedCard == null)
            {
                Debug.LogWarning("[CardDropToSlotHandler] 드롭할 카드가 없습니다.");
                return;
            }

            if (slot == null)
            {
                Debug.LogError("[CardDropToSlotHandler] 슬롯이 초기화되지 않았습니다.");
                return;
            }

            // 슬롯에 카드 할당
            slot.SetCard(draggedCard);
            Debug.Log($"[CardDropToSlotHandler] 카드가 슬롯에 드롭됨: {draggedCard.GetCardName()}");

            // 이벤트 브로드캐스트
            CardDropEventSystem.NotifyCardDropped(draggedCard, slot);

            // 드래그 종료
            CardDragHandler.Clear();
        }
    }
}
