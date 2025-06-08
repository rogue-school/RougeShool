using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드가 전투 슬롯에 드롭 가능한지를 검사하는 인터페이스입니다.
    /// 카드의 조건, 슬롯의 상태 등을 기준으로 유효성 검사를 수행합니다.
    /// </summary>
    public interface ICardDropValidator
    {
        /// <summary>
        /// 지정된 카드가 해당 슬롯에 드롭 가능한지를 판별합니다.
        /// </summary>
        /// <param name="card">드롭 시도 중인 스킬 카드</param>
        /// <param name="slot">드롭 대상 슬롯</param>
        /// <param name="reason">
        /// 드롭이 불가능한 경우 사유 메시지를 반환합니다. (null 또는 빈 문자열이면 사유 미제공)
        /// </param>
        /// <returns>true: 드롭 가능, false: 드롭 불가</returns>
        bool IsValidDrop(ISkillCard card, ICombatCardSlot slot, out string reason);
    }
}
