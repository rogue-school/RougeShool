using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드 슬롯에 스킬 카드를 배치하는 기능을 제공하는 인터페이스입니다.
    /// UI와 슬롯을 함께 고려하여 배치를 처리합니다.
    /// </summary>
    public interface ICardPlacementService
    {
        /// <summary>
        /// 스킬 카드를 지정된 슬롯에 배치합니다.
        /// </summary>
        /// <param name="card">배치할 스킬 카드</param>
        /// <param name="ui">카드에 대응하는 UI 오브젝트</param>
        /// <param name="slot">카드를 배치할 전투 슬롯</param>
        void PlaceCardInSlot(ISkillCard card, ISkillCardUI ui, ICombatCardSlot slot);
    }
}
