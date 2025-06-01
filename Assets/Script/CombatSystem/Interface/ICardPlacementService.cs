using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICardPlacementService
    {
        // 기존에는 SkillCardUI였음 → 인터페이스 사용
        void PlaceCardInSlot(ISkillCard card, ISkillCardUI ui, ICombatCardSlot slot);
    }
}
