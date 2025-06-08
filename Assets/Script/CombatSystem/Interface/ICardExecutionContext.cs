using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 스킬 카드 실행 중 필요한 실행 정보를 포함하는 컨텍스트 인터페이스입니다.
    /// 카드, 시전자, 대상자, 플레이어/적 캐릭터 참조 등을 제공합니다.
    /// </summary>
    public interface ICardExecutionContext
    {
        /// <summary>
        /// 실행 중인 스킬 카드 객체입니다.
        /// </summary>
        ISkillCard Card { get; }

        /// <summary>
        /// 스킬을 시전한 캐릭터입니다.
        /// </summary>
        ICharacter Source { get; }

        /// <summary>
        /// 스킬의 대상 캐릭터입니다.
        /// </summary>
        ICharacter Target { get; }

        /// <summary>
        /// 컨텍스트에서 현재 플레이어 캐릭터를 반환합니다.
        /// </summary>
        IPlayerCharacter GetPlayer();

        /// <summary>
        /// 컨텍스트에서 현재 적 캐릭터를 반환합니다.
        /// </summary>
        IEnemyCharacter GetEnemy();
    }
}
