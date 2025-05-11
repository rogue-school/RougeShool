using Game.Interface;

namespace Game.Effect
{
    /// <summary>
    /// 매 턴마다 실행되는 지속 효과 인터페이스입니다.
    /// </summary>
    public interface IPerTurnEffect
    {
        /// <summary>
        /// 턴 시작 시 실행되는 메서드입니다.
        /// </summary>
        /// <param name="owner">효과를 적용받는 캐릭터</param>
        void OnTurnStart(ICharacter owner);

        /// <summary>
        /// 효과가 만료되었는지 여부를 반환합니다.
        /// </summary>
        bool IsExpired { get; }
    }
}
