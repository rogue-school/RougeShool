using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 스킬 카드 실행 관련 통합 인터페이스입니다.
    /// 카드 실행 전 유효성 검사와 실행 로직을 모두 담당합니다.
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

        /// <summary>
        /// 지정된 스킬 카드와 실행 컨텍스트를 사용하여 카드의 효과를 실행합니다.
        /// </summary>
        /// <param name="card">실행할 <see cref="ISkillCard"/> 객체입니다.</param>
        /// <param name="context">
        /// 카드 실행에 필요한 정보를 포함하는 <see cref="ICardExecutionContext"/> 객체입니다.
        /// 예: 시전자, 대상자, 카드 데이터 등
        /// </param>
        void Execute(ISkillCard card, ICardExecutionContext context);
    }
}
