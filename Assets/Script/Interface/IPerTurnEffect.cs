using Game.Interface;

namespace Game.Effect
{
    /// <summary>
    /// 매 턴마다 실행되는 지속 효과 인터페이스입니다.
    /// </summary>
    public interface IPerTurnEffect
    {
        /// <summary>
        /// 턴 시작 시 효과를 적용합니다.
        /// </summary>
        /// <param name="target">효과가 적용되는 대상</param>
        void ApplyPerTurn(ICharacter target);

        /// <summary>
        /// 해당 효과가 만료되었는지 여부를 반환합니다.
        /// </summary>
        bool IsFinished();
    }
}
