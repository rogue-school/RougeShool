using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Interface;
using UnityEngine;
using Game.CombatSystem.Context;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.SkillCardSystem.Executor;

namespace Game.CombatSystem.Service
{
    public class CardExecutionContextProvider : ICardExecutionContextProvider
    {
        private readonly IPlayerManager playerManager;
        private readonly IEnemyManager enemyManager;

        public CardExecutionContextProvider(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
        }

        public ICardExecutionContext CreateContext(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogError("[ContextProvider] 카드가 null입니다.");
                return null;
            }

            ICharacter source, target;

            switch (card.GetOwner())
            {
                case SlotOwner.PLAYER:
                    source = playerManager.GetPlayer();
                    target = enemyManager.GetEnemy();
                    break;

                case SlotOwner.ENEMY:
                    source = enemyManager.GetEnemy();
                    target = playerManager.GetPlayer();
                    break;

                default:
                    Debug.LogError("[ContextProvider] 알 수 없는 소유자");
                    return null;
            }

            return new DefaultCardExecutionContext(card, source, target);
        }
    }
}
