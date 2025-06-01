using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.DragDrop
{
    public class DefaultCardDropValidator : ICardDropValidator
    {
        public bool IsValidDrop(ISkillCard card, ICombatCardSlot slot, out string reason)
        {
            reason = null;

            if (card == null || slot == null)
            {
                reason = "카드 또는 슬롯이 null입니다.";
                return false;
            }

            if (!card.IsFromPlayer())
            {
                reason = "플레이어 카드가 아닙니다.";
                return false;
            }

            var existing = slot.GetCard();
            if (existing != null && !existing.IsFromPlayer())
            {
                reason = "슬롯에 적 카드가 있어 드롭할 수 없습니다.";
                return false;
            }

            return true;
        }
    }
}
