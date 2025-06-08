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
    /// <summary>
    /// 카드 실행 컨텍스트를 제공하는 서비스입니다.
    /// 카드의 소유자에 따라 실행에 필요한 소스/타겟 정보를 설정합니다.
    /// </summary>
    public class CardExecutionContextProvider : ICardExecutionContextProvider
    {
        #region 필드

        private readonly IPlayerManager playerManager;
        private readonly IEnemyManager enemyManager;

        #endregion

        #region 생성자

        /// <summary>
        /// 생성자에서 의존성 주입을 통해 매니저를 초기화합니다.
        /// </summary>
        public CardExecutionContextProvider(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
        }

        #endregion

        #region 컨텍스트 생성

        /// <summary>
        /// 카드 정보를 바탕으로 실행 컨텍스트를 생성합니다.
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <returns>컨텍스트 정보</returns>
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

        #endregion
    }
}
