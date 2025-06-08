using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 스킬 카드 실행 전 유효성 검사를 수행하는 인터페이스입니다.
    /// 카드 실행 가능 여부를 판단하여 실행 전에 조건을 제한합니다.
    /// </summary>
    public interface ICardExecutionValidator
    {
        /// <summary>
        /// 주어진 카드가 현재 컨텍스트에서 실행 가능한지 여부를 반환합니다.
        /// </summary>
        /// <param name="card">검사 대상 스킬 카드.</param>
        /// <param name="context">실행 컨텍스트 (시전자, 대상 등 포함).</param>
        /// <returns>true면 실행 가능, false면 제한됨.</returns>
        bool CanExecute(ISkillCard card, ICardExecutionContext context);
    }
}
