using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Utility
{
    public static class CardValidator
    {
        public static bool IsPlayerCard(ISkillCard card)
        {
            var handSlot = card?.GetHandSlot();
            return handSlot.HasValue && handSlot.Value.ToString().Contains("PLAYER");
        }
    }
}