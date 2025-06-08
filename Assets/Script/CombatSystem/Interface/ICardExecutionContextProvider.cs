using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 스킬 카드 실행에 필요한 컨텍스트(<see cref="ICardExecutionContext"/>)를 생성하는 인터페이스입니다.
    /// 카드 실행 시 사용될 시전자, 대상자 등의 정보를 포함한 컨텍스트를 제공합니다.
    /// </summary>
    public interface ICardExecutionContextProvider
    {
        /// <summary>
        /// 주어진 스킬 카드에 대해 실행 컨텍스트를 생성합니다.
        /// </summary>
        /// <param name="card">실행 대상 스킬 카드</param>
        /// <returns>
        /// 카드 실행에 필요한 <see cref="ICardExecutionContext"/> 객체.
        /// 시전자 및 대상자가 포함됩니다.
        /// </returns>
        ICardExecutionContext CreateContext(ISkillCard card);
    }
}
