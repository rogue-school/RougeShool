using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 슬롯에 카드와 UI를 등록하는 기능을 담당하는 인터페이스입니다.
    /// 슬롯 상태, 카드 정보, UI 오브젝트를 함께 연결합니다.
    /// </summary>
    public interface ICardRegistrar
    {
        /// <summary>
        /// 지정된 슬롯에 카드와 UI를 등록합니다.
        /// 등록 후 슬롯이 해당 카드/UI 정보를 보관하게 됩니다.
        /// </summary>
        /// <param name="slot">카드가 배치되는 슬롯</param>
        /// <param name="card">등록할 스킬 카드</param>
        /// <param name="ui">등록할 카드의 UI 오브젝트</param>
        void RegisterCard(ICombatCardSlot slot, ISkillCard card, SkillCardUI ui);
    }
}
