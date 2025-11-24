using System;
using Game.Domain.Character.Entities;
using Game.Domain.Character.Interfaces;
using Game.Domain.Character.ValueObjects;

namespace Game.Application.Character
{
    /// <summary>
    /// 도메인 캐릭터 엔티티를 초기화하는 애플리케이션 유스케이스입니다.
    /// Unity 컴포넌트나 ScriptableObject에 의존하지 않고 순수 도메인 객체만 생성합니다.
    /// </summary>
    public sealed class InitializeCharacterUseCase
    {
        /// <summary>
        /// 플레이어 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="id">캐릭터 ID</param>
        /// <param name="name">표시 이름</param>
        /// <param name="stats">초기 체력 정보</param>
        /// <param name="resource">초기 리소스 정보</param>
        /// <returns>생성된 플레이어 캐릭터 도메인 객체</returns>
        /// <exception cref="ArgumentException">ID 또는 이름이 비어 있는 경우</exception>
        public IPlayerCharacter CreatePlayer(
            string id,
            string name,
            CharacterStats stats,
            Resource resource)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("캐릭터 ID는 비어 있을 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("캐릭터 이름은 비어 있을 수 없습니다.", nameof(name));
            }

            return new PlayerCharacter(id, name, stats, resource);
        }

        /// <summary>
        /// 적 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="id">캐릭터 ID</param>
        /// <param name="name">표시 이름</param>
        /// <param name="stats">초기 체력 정보</param>
        /// <returns>생성된 적 캐릭터 도메인 객체</returns>
        /// <exception cref="ArgumentException">ID 또는 이름이 비어 있는 경우</exception>
        public IEnemyCharacter CreateEnemy(
            string id,
            string name,
            CharacterStats stats)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("캐릭터 ID는 비어 있을 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("캐릭터 이름은 비어 있을 수 없습니다.", nameof(name));
            }

            return new EnemyCharacter(id, name, stats);
        }
    }
}


