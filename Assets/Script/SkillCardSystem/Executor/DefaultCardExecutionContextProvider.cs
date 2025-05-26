using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using UnityEngine;
using Game.SkillCardSystem.Executor;
using System;

namespace Game.CombatSystem.Context
{
    /// <summary>
    /// 카드 소유자에 따라 Source/Target을 자동 할당하는 기본 실행 컨텍스트 생성기
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
            if (card == null)
                throw new ArgumentNullException(nameof(card), "[ExecutionContextProvider] 카드가 null입니다.");

            var source = GetSourceCharacter(card);
            var target = GetTargetCharacter(card);

            if (source == null || target == null)
            {
                Debug.LogError("[ExecutionContextProvider] Source 또는 Target이 null입니다. 카드 실행 취소.");
                return null;
            }

            return new DefaultCardExecutionContext(card, source, target);
        }

        private ICharacter GetSourceCharacter(ISkillCard card)
        {
            return card.IsFromPlayer() ? player : enemy;
        }

        private ICharacter GetTargetCharacter(ISkillCard card)
        {
            return card.IsFromPlayer() ? enemy : player;
        }
    }
}
