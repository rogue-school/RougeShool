using UnityEngine;
using Game.CharacterSystem.Data;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터 생성을 위한 팩토리 패턴 인터페이스
    /// Strategy Pattern과 Factory Pattern을 결합하여 다양한 캐릭터 생성 전략을 지원
    /// </summary>
    public interface ICharacterFactory
    {
        /// <summary>
        /// 플레이어 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="data">캐릭터 데이터</param>
        /// <param name="position">생성 위치</param>
        /// <returns>생성된 플레이어 캐릭터</returns>
        IPlayerCharacter CreatePlayerCharacter(PlayerCharacterData data, Vector3 position);

        /// <summary>
        /// 적 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="data">캐릭터 데이터</param>
        /// <param name="position">생성 위치</param>
        /// <returns>생성된 적 캐릭터</returns>
        IEnemyCharacter CreateEnemyCharacter(EnemyCharacterData data, Vector3 position);

        /// <summary>
        /// 캐릭터를 특정 전략으로 생성합니다.
        /// </summary>
        /// <param name="data">캐릭터 데이터</param>
        /// <param name="position">생성 위치</param>
        /// <param name="strategy">생성 전략</param>
        /// <returns>생성된 캐릭터</returns>
        ICharacter CreateCharacter(ICharacterData data, Vector3 position, ICharacterCreationStrategy strategy);
    }

    /// <summary>
    /// 캐릭터 생성 전략 인터페이스 (Strategy Pattern)
    /// </summary>
    public interface ICharacterCreationStrategy
    {
        /// <summary>
        /// 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="data">캐릭터 데이터</param>
        /// <param name="position">생성 위치</param>
        /// <returns>생성된 캐릭터</returns>
        ICharacter CreateCharacter(ICharacterData data, Vector3 position);
    }

    /// <summary>
    /// 캐릭터 데이터의 공통 인터페이스
    /// </summary>
    public interface ICharacterData
    {
        string DisplayName { get; }
        int MaxHP { get; }
        Sprite Portrait { get; }
    }
} 