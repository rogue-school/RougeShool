using Game.CharacterSystem.Data;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 적 캐릭터 전용 기능 인터페이스입니다.
    /// ICharacter를 상속하며, 적 전용 데이터 접근 및 이름 반환 기능을 포함합니다.
    /// </summary>
    public interface IEnemyCharacter : ICharacter
    {
        /// <summary>
        /// 적 캐릭터의 데이터 스크립터블 객체를 반환합니다.
        /// </summary>
        EnemyCharacterData Data { get; }

        /// <summary>
        /// 적 캐릭터의 이름을 반환합니다.
        /// UI 또는 로그에서 사용되는 표기 전용 이름입니다.
        /// </summary>
        /// <returns>적 캐릭터의 이름</returns>
        string GetName();
    }
}
