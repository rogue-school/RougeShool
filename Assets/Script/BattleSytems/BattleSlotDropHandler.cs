using UnityEngine;
using UnityEngine.EventSystems;
using Game.UI;
using Game.Cards;

namespace Game.Battle
{
    public class BattleSlotDropHandler : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            var droppedCardUI = eventData.pointerDrag?.GetComponent<CardUI>();
            if (droppedCardUI == null) return;

            var cardData = droppedCardUI.CardData;

            Debug.Log($"카드 드롭됨: {cardData.cardName}");

            // 전투 슬롯에 카드 올리는 처리 추가 예정
        }
    }
}
