using Game.SkillCardSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Context
{
    /// <summary>
    /// 기본 카드 실행 컨텍스트 생성기 - 플레이어와 적 단일 대상 기준
    /// </summary>
    public class DefaultCardExecutionContextProvider : ICardExecutionContextProvider
    {
        private readonly IPlayerManager playerManager;
        private readonly IEnemyManager enemyManager;

        public DefaultCardExecutionContextProvider(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
        }

        public ICardExecutionContext CreateContext(ISkillCard card)
        {
            var source = playerManager?.GetPlayer();
            var target = enemyManager?.GetEnemy(); // 단일 적 반환 메서드

            if (source == null || target == null)
            {
                UnityEngine.Debug.LogWarning("[DefaultCardExecutionContextProvider] 시전자 또는 대상자 정보가 누락되었습니다.");
            }

            return new DefaultCardExecutionContext(card, source, target);
        }
    }
}
