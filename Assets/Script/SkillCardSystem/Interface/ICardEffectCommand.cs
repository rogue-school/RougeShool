using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 스킬 카드 이펙트 실행 커맨드 인터페이스입니다.
    /// 카드 실행 시점에 생성되어, 지정된 컨텍스트와 함께 이펙트를 수행합니다.
    /// </summary>
    public interface ICardEffectCommand
    {
        /// <summary>
        /// 카드 이펙트를 실행합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트. 시전자, 대상, 카드 정보 등을 포함합니다.</param>
        /// <param name="turnManager">전투 턴 관리자. 가드 처리, 상태 변경 등에서 사용됩니다.</param>
        void Execute(ICardExecutionContext context, ICombatTurnManager turnManager);
    }
}
