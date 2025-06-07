using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Utility
{
    public static class CardRegistrar
    {
        public static void ClearSlot(ICombatCardSlot slot)
        {
            var oldUI = slot.GetCardUI() as SkillCardUI;
            if (oldUI != null)
            {
                GameObject.Destroy(oldUI.gameObject);
            }

            slot.SetCard(null);
            slot.SetCardUI(null);

            Debug.Log($"[CardRegistrar] 슬롯 클리어 완료: {slot.GetCombatPosition()}");
        }

        public static void RegisterCard(ICombatCardSlot slot, ISkillCard card, SkillCardUI ui)
        {
            var execSlot = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
            card.SetCombatSlot(execSlot);

            slot.SetCard(card);
            slot.SetCardUI(ui);

            CardSlotHelper.AttachCardToSlot(ui, (MonoBehaviour)slot);

            Debug.Log($"[CardRegistrar] 카드 등록 완료: {card.CardData.Name} → {execSlot}");
        }
    }
}
