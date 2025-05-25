using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드와 UI를 전투 슬롯에 배치하는 책임을 담당합니다.
    /// </summary>
    public interface ICardPlacementService
    {
        void PlaceCardInSlot(ISkillCard card, SkillCardUI ui, ICombatCardSlot slot);
    }
}
