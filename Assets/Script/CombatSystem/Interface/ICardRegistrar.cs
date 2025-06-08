using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 슬롯에 스킬 카드와 그에 대응하는 UI를 등록하는 기능을 제공합니다.
    /// 슬롯은 등록된 카드와 UI를 기반으로 실행 및 시각적 처리를 수행합니다.
    /// </summary>
    public interface ICardRegistrar
    {
        /// <summary>
        /// 지정된 전투 슬롯에 스킬 카드 및 해당 UI를 등록합니다.
        /// 등록된 슬롯은 카드 실행이나 상태 갱신에 이 정보를 사용합니다.
        /// </summary>
        /// <param name="slot">
        /// <see cref="ICombatCardSlot"/> 인터페이스를 구현하는 슬롯 객체.
        /// 스킬 카드가 배치되는 대상 슬롯입니다.
        /// </param>
        /// <param name="card">
        /// <see cref="ISkillCard"/> 객체. 슬롯에 등록될 실제 카드 데이터입니다.
        /// </param>
        /// <param name="ui">
        /// <see cref="SkillCardUI"/> 컴포넌트. 카드에 대응하는 UI 오브젝트로,
        /// 시각적 효과 및 드래그 관련 처리를 담당합니다.
        /// </param>
        void RegisterCard(ICombatCardSlot slot, ISkillCard card, SkillCardUI ui);
    }
}
