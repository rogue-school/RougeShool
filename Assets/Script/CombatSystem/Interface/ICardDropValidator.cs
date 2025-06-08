using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드가 전투 슬롯에 드롭 가능한지를 검사하는 인터페이스입니다.
    /// 카드의 소유자, 슬롯 상태, 쿨타임 등 다양한 조건을 기반으로 유효성을 판단합니다.
    /// </summary>
    public interface ICardDropValidator
    {
        /// <summary>
        /// 지정된 카드가 주어진 슬롯에 드롭 가능한지를 판별합니다.
        /// </summary>
        /// <param name="card">드롭을 시도하는 스킬 카드</param>
        /// <param name="slot">드롭 대상 슬롯</param>
        /// <param name="reason">
        /// 드롭이 불가능한 경우 사유를 문자열로 반환합니다.
        /// null 또는 빈 문자열일 경우, 사유가 명시되지 않은 것입니다.
        /// </param>
        /// <returns>
        /// true이면 드롭이 허용되고, false이면 드롭이 차단됩니다.
        /// </returns>
        bool IsValidDrop(ISkillCard card, ICombatCardSlot slot, out string reason);
    }
}
