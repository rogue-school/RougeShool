using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Utility;
using UnityEngine;

namespace Game.CombatSystem.Utility
{
    public static class CardRegistrar
    {
        public static void RegisterCard(ICombatCardSlot slot, ISkillCard card, SkillCardUI cardUI)
        {
            if (slot == null || card == null || cardUI == null)
            {
                Debug.LogError("[CardRegistrar] 등록 대상 중 null 있음 (slot/card/cardUI)");
                return;
            }

            var execPosition = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
            card.SetCombatSlot(execPosition);

            slot.SetCard(card);
            slot.SetCardUI(cardUI);

            Debug.Log($"[CardRegistrar] 카드 등록 완료: {card.CardData.Name} → {execPosition}");
        }

        public static void ClearSlot(ICombatCardSlot slot)
        {
            if (slot == null)
            {
                Debug.LogWarning("[CardRegistrar] ClearSlot 대상이 null입니다.");
                return;
            }

            slot.SetCard(null);
            slot.SetCardUI(null);

            Debug.Log($"[CardRegistrar] 슬롯 클리어 완료: {slot.GetCombatPosition()}");
        }
    }
}
