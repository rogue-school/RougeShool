using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 카드 실행 컨텍스트(ICardExecutionContext)를 생성하는 팩토리 인터페이스입니다.
    /// </summary>
    public interface ICardExecutionContextProvider
    {
        ICardExecutionContext CreateContext(ISkillCard card);
    }
}
