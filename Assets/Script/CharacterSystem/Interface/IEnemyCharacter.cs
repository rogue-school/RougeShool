using Game.CharacterSystem.Data;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 적 캐릭터 전용 기능 인터페이스입니다.
    /// </summary>
    public interface IEnemyCharacter : ICharacter
    {
        EnemyCharacterData Data { get; }
    }
}
