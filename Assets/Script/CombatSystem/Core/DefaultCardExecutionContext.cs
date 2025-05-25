using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Context
{
    /// <summary>
    /// 카드 실행 시 사용되는 기본 실행 컨텍스트.
    /// 카드, 시전자, 대상자 정보를 포함합니다.
    /// </summary>
    public class DefaultCardExecutionContext : ICardExecutionContext
    {
        public ISkillCard Card { get; }
        public ICharacter Source { get; }
        public ICharacter Target { get; }

        public DefaultCardExecutionContext(ISkillCard card, ICharacter source, ICharacter target)
        {
            Card = card;
            Source = source;
            Target = target;
        }

        public IPlayerCharacter GetPlayer()
        {
            return Source is IPlayerCharacter player ? player :
                   Target is IPlayerCharacter targetPlayer ? targetPlayer : null;
        }

        public IEnemyCharacter GetEnemy()
        {
            return Source is IEnemyCharacter enemy ? enemy :
                   Target is IEnemyCharacter targetEnemy ? targetEnemy : null;
        }
    }
}
