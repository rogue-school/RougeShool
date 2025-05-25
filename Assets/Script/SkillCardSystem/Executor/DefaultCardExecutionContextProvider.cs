using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using UnityEngine;
using Game.SkillCardSystem.Executor;

namespace Game.CombatSystem.Context
{
    /// <summary>
    /// 카드의 소유자(플레이어/적)에 따라 자동으로 Source/Target을 설정해주는 기본 컨텍스트 제공자.
    /// </summary>
    public class DefaultCardExecutionContextProvider : ICardExecutionContextProvider
    {
        private readonly IPlayerCharacter player;
        private readonly IEnemyCharacter enemy;

        public DefaultCardExecutionContextProvider(IPlayerCharacter player, IEnemyCharacter enemy)
        {
            this.player = player;
            this.enemy = enemy;
        }

        public ICardExecutionContext CreateContext(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogError("[DefaultCardExecutionContextProvider] 카드가 null입니다.");
                return null;
            }

            ICharacter source = card.IsFromPlayer() ? player : enemy;
            ICharacter target = card.IsFromPlayer() ? enemy : player;

            return new DefaultCardExecutionContext(card, source, target);
        }
    }
}
