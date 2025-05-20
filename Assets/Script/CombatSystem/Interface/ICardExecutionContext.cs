using Game.CharacterSystem.Core;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드 이펙트 실행 시 필요한 컨텍스트 정보 (시전자 / 대상자 등)를 제공합니다.
    /// </summary>
    public interface ICardExecutionContext
    {
        /// <summary>
        /// 플레이어 캐릭터를 반환합니다.
        /// </summary>
        IPlayerCharacter GetPlayer();

        /// <summary>
        /// 적 캐릭터를 반환합니다.
        /// </summary>
        IEnemyCharacter GetEnemy();

        /// <summary>
        /// 현재 카드 실행의 시전자 캐릭터를 반환합니다.
        /// </summary>
        ICharacter GetSourceCharacter();

        /// <summary>
        /// 현재 카드 실행의 대상 캐릭터를 반환합니다.
        /// </summary>
        ICharacter GetTargetCharacter();
    }
}
