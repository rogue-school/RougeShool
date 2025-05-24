using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 전투 슬롯에 카드를 배치하는 기능을 담당합니다.
    /// </summary>
    public class CardPlacementService : ICardPlacementService
    {
        public void PlaceCardInSlot(ISkillCard card, SkillCardUI ui, ICombatCardSlot slot)
        {
            if (card == null || ui == null || slot == null)
            {
                Debug.LogError("[CardPlacementService] 카드, UI, 슬롯 중 하나 이상이 null입니다.");
                return;
            }

            slot.SetCard(card);
            slot.SetCardUI(ui);

            var t = ((MonoBehaviour)slot).transform;
            ui.transform.SetParent(t);
            ui.transform.localPosition = Vector3.zero;
            ui.transform.localScale = Vector3.one;

            Debug.Log($"[CardPlacementService] 카드 '{card.GetCardName()}' 슬롯 {slot.GetCombatPosition()}에 배치 완료");
        }
    }
}
