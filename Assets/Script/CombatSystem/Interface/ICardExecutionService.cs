using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 스킬 카드 실행 로직을 담당하는 서비스 인터페이스입니다.
    /// 카드 및 실행 컨텍스트를 기반으로 스킬 효과를 적용합니다.
    /// </summary>
    public interface ICardExecutionService
    {
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
