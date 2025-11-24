using Game.Domain.Character.Interfaces;
using Game.Domain.Character.ValueObjects;

namespace Game.Domain.Character.Entities
{
    /// <summary>
    /// 적 캐릭터의 도메인 엔티티입니다.
    /// </summary>
    public sealed class EnemyCharacter : Character, IEnemyCharacter
    {
        /// <summary>
        /// 적 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="id">캐릭터 ID</param>
        /// <param name="name">표시 이름</param>
        /// <param name="initialStats">초기 체력 정보</param>
        public EnemyCharacter(
            string id,
            string name,
            CharacterStats initialStats)
            : base(id, name, initialStats)
        {
        }
    }
}


