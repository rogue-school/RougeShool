using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICardExecutionContext
    {
        ISkillCard Card { get; }
        ICharacter Source { get; }
        ICharacter Target { get; }

        // 선택적 유틸리티
        IPlayerCharacter GetPlayer();  // Source 또는 Target 중 플레이어
        IEnemyCharacter GetEnemy();    // Source 또는 Target 중 적
    }
}
