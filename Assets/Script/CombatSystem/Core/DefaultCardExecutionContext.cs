using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 기본 카드 실행 컨텍스트 구현.
    /// 카드 인스턴스로부터 실행 주체(Owner)와 대상(Target)을 유추합니다.
    /// </summary>
    public class DefaultCardExecutionContext : ICardExecutionContext
    {
        private readonly ISkillCard card;

        public DefaultCardExecutionContext(ISkillCard card)
        {
            this.card = card;
        }

        public ISkillCard GetCard() => card;

        public IPlayerCharacter GetPlayer()
        {
            return GetSourceCharacter() as IPlayerCharacter;
        }

        public IEnemyCharacter GetEnemy()
        {
            return GetTargetCharacter() as IEnemyCharacter;
        }

        public ICharacter GetSourceCharacter()
        {
            return card.GetOwner(this);
        }

        public ICharacter GetTargetCharacter()
        {
            return card.GetTarget(this);
        }
    }
}
