using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Context
{
    public class DefaultCardExecutionContext : ICardExecutionContext
    {
        public ISkillCard Card { get; private set; }
        public ICharacter Source { get; private set; }
        public ICharacter Target { get; private set; }

        public DefaultCardExecutionContext(ISkillCard card, ICharacter source, ICharacter target)
        {
            Card = card;
            Source = source;
            Target = target;
        }

        public IPlayerCharacter GetPlayer()
        {
            return Source is IPlayerCharacter player ? player :
                   Target is IPlayerCharacter tPlayer ? tPlayer : null;
        }

        public IEnemyCharacter GetEnemy()
        {
            return Source is IEnemyCharacter enemy ? enemy :
                   Target is IEnemyCharacter tEnemy ? tEnemy : null;
        }
    }
}
