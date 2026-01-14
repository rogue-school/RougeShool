using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 스킬 카드 실행 중 필요한 모든 정보를 포함하는 실행 컨텍스트 인터페이스입니다.
    /// 실행 대상 카드, 시전자(Source), 대상자(Target) 및 플레이어/적 캐릭터 참조를 제공합니다.
    /// </summary>
    public interface ICardExecutionContext
    {
        /// <summary>
        /// 현재 실행 중인 스킬 카드입니다.
        /// </summary>
        ISkillCard Card { get; }

        /// <summary>
        /// 스킬을 시전하는 캐릭터입니다.
        /// 일반적으로 플레이어 또는 적 캐릭터입니다.
        /// </summary>
        ICharacter Source { get; }

        /// <summary>
        /// 스킬의 대상이 되는 캐릭터입니다.
        /// 보통 Source와 반대 진영에 속합니다.
        /// </summary>
        ICharacter Target { get; }

        /// <summary>
        /// 컨텍스트에서 현재 플레이어 캐릭터를 반환합니다.
        /// Source 또는 Target 중 해당되는 플레이어가 있으면 반환됩니다.
        /// </summary>
        ICharacter GetPlayer();

        /// <summary>
        /// 컨텍스트에서 현재 적 캐릭터를 반환합니다.
        /// Source 또는 Target 중 해당되는 적이 있으면 반환됩니다.
        /// </summary>
        ICharacter GetEnemy();
    }
}
