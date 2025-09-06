using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 스킬 카드의 실행을 담당하는 실행기 인터페이스입니다.
    /// 실제 카드 효과를 적용하고, 실행 컨텍스트와 턴 매니저를 활용합니다.
    /// </summary>
    public interface ICardExecutor
    {
        /// <summary>
        /// 지정된 카드와 실행 컨텍스트를 기반으로 효과를 실행합니다.
        /// 실행 이후 쿨타임 처리나 턴 상태 변경 등을 관리할 수 있습니다.
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <param name="context">실행에 필요한 정보가 담긴 컨텍스트</param>
        /// <param name="turnManager">전투 턴 상태를 관리하는 매니저</param>
        void Execute(ISkillCard card, ICardExecutionContext context, ICombatTurnManager turnManager);
    }
}
