using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 스킬 카드를 전투 슬롯에 배치하는 서비스를 정의합니다.
    /// 카드 UI와 슬롯 상태를 함께 고려하여 배치 동작을 수행합니다.
    /// </summary>
    public interface ICardPlacementService
    {
        /// <summary>
        /// 지정된 스킬 카드를 전투 슬롯에 배치합니다.
        /// 카드 UI는 시각적 위치 이동이나 상태 업데이트 등에 활용됩니다.
        /// </summary>
        /// <param name="card">배치할 <see cref="ISkillCard"/> 객체</param>
        /// <param name="ui">
        /// 해당 카드에 연결된 <see cref="ISkillCardUI"/> 인스턴스.
        /// UI 이동 또는 시각적 효과 반영에 사용됩니다.
        /// </param>
        /// <param name="slot">
        /// 카드를 배치할 대상 슬롯. <see cref="ICombatCardSlot"/>을 구현한 슬롯 객체입니다.
        /// </param>
        void PlaceCardInSlot(ISkillCard card, ISkillCardUI ui, ICombatCardSlot slot);
    }
}
