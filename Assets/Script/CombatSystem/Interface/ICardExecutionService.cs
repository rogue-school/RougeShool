using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 스킬 카드를 실행하는 서비스를 정의하는 인터페이스입니다.
    /// 카드와 컨텍스트를 기반으로 효과를 적용합니다.
    /// </summary>
    public interface ICardExecutionService
    {
        /// <summary>
        /// 주어진 카드와 실행 컨텍스트를 바탕으로 카드의 효과를 실행합니다.
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <param name="context">카드 실행에 필요한 정보(시전자, 대상자 등)</param>
        void Execute(ISkillCard card, ICardExecutionContext context);
    }
}
