using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 스킬 카드 실행 시 사용할 컨텍스트(ICardExecutionContext)를 생성하는 제공자 인터페이스입니다.
    /// </summary>
    public interface ICardExecutionContextProvider
    {
        /// <summary>
        /// 주어진 카드에 대한 실행 컨텍스트를 생성합니다.
        /// </summary>
        /// <param name="card">실행 대상 스킬 카드</param>
        /// <returns>카드 실행에 필요한 컨텍스트 객체</returns>
        ICardExecutionContext CreateContext(ISkillCard card);
    }
}
