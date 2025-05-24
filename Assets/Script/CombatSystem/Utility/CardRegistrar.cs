using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Utility
{
    public static class CardRegistrar
    {
        public static void RegisterCard(
            ICombatCardSlot slot,
            ISkillCard card,
            SkillCardUI cardUI)
        {
            var convertedPosition = SlotPositionUtil.ToExecutionSlot(slot.GetCombatPosition());
            card.SetCombatSlot(convertedPosition);

            slot.SetCard(card);
            slot.SetCardUI(cardUI);
        }

        public static void ClearSlot(ICombatCardSlot slot)
        {
            slot.SetCard(null);
            slot.SetCardUI(null);
        }
    }
}
