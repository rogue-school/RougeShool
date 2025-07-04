using UnityEngine;
using UnityEngine.EventSystems;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem.UI;
using Game.CombatSystem.Utility;
using Game.CombatSystem.Slot;
using System.Linq;
using Game.SkillCardSystem.Core;

namespace Game.CombatSystem.DragDrop
{
    public class CardDropToHandHandler : MonoBehaviour, IDropHandler
    {
        [SerializeField] private PlayerHandManager handManager;

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("[CardDropToHandHandler] 드롭 감지됨");

            var cardUI = eventData.pointerDrag?.GetComponent<SkillCardUI>();
            var dragHandler = eventData.pointerDrag?.GetComponent<CardDragHandler>();
            var card = cardUI?.GetCard();

            if (cardUI == null || dragHandler == null || card == null)
                return;

            var combatSlot = card.GetCombatSlot();
            if (combatSlot != CombatSlotPosition.NONE)
            {
                var slots = Object.FindObjectsByType<CombatExecutionSlotUI>(FindObjectsSortMode.None);
                var targetSlot = slots.FirstOrDefault(s => s.Position == combatSlot);
                targetSlot?.ClearAll();
            }

            handManager.RestoreCardToHand(card);
            CardSlotHelper.ResetCardToOriginal(cardUI);

            dragHandler.OriginalParent = this.transform;
            dragHandler.OriginalWorldPosition = this.transform.position;

            Debug.Log($"[CardDropToHandHandler] 카드 핸드 복귀 완료: {card.CardData.Name}");
        }
    }
}
