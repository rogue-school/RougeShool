using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    /// <summary>
    /// 스킬 카드 이펙트 실행 커맨드 인터페이스.
    /// 이펙트가 실행되어야 할 때, source/target/context/controller를 받아 수행됨.
    /// </summary>
    public interface ICardEffectCommand
    {
        /// <summary>
        /// 이펙트를 실행합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트 (source, target 등 포함)</param>
        /// <param name="controller">턴 상태 컨트롤러 (가드/슬롯 변경 등)</param>
        void Execute(ICardExecutionContext context, ITurnStateController controller);
    }
}
