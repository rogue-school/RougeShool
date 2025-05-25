using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 카드 실행 중 필요한 소유자, 대상자, 카드 정보를 제공하는 컨텍스트 인터페이스.
    /// </summary>
    public interface ICardExecutionContext
    {
        ISkillCard Card { get; }
        ICharacter Source { get; }
        ICharacter Target { get; }

        IPlayerCharacter GetPlayer();
        IEnemyCharacter GetEnemy();
    }
}
