using System;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Context;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 카드 소유자에 따라 Source/Target을 자동 추론하여 컨텍스트를 생성합니다.
    /// </summary>
    public class DefaultCardExecutionContextProvider : ICardExecutionContextProvider
    {
        private readonly IPlayerCharacter player;
        private readonly IEnemyCharacter enemy;

        public DefaultCardExecutionContextProvider(IPlayerCharacter player, IEnemyCharacter enemy)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
        }

        public ICardExecutionContext CreateContext(ISkillCard card)
        {
            if (card == null) return null;

            ICharacter source = card.IsFromPlayer() ? (ICharacter)player : enemy;
            ICharacter target = card.IsFromPlayer() ? (ICharacter)enemy : player;

            return new DefaultCardExecutionContext(card, source, target);
        }

    }
}
