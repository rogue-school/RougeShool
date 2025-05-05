using Game.Characters;

namespace Game.Interface
{
    /// <summary>
    /// 매 턴마다 실행되는 지속 효과 인터페이스입니다.
    /// </summary>
    public interface IPerTurnEffect
    {
        /// <summary>
        /// 턴 시작 시 실행되는 메서드입니다.
        /// </summary>
        /// <param name="owner">효과를 보유한 캐릭터</param>
        void OnTurnStart(CharacterBase owner);

        /// <summary>
        /// 효과의 만료 여부를 반환합니다.
        /// </summary>
        bool IsExpired { get; }
    }
}
