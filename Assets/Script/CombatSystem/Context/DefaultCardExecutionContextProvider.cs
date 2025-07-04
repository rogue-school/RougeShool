using Game.SkillCardSystem.Interface;
using Game.IManager;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Context
{
    /// <summary>
    /// 기본 카드 실행 컨텍스트 제공자입니다.
    /// 플레이어와 단일 적 캐릭터를 기준으로 컨텍스트를 생성합니다.
    /// </summary>
    public class DefaultCardExecutionContextProvider : ICardExecutionContextProvider
    {
        private readonly IPlayerManager playerManager;
        private readonly IEnemyManager enemyManager;

        /// <summary>
        /// 생성자에서 플레이어 및 적 매니저를 주입받습니다.
        /// </summary>
        /// <param name="playerManager">플레이어 매니저</param>
        /// <param name="enemyManager">적 매니저</param>
        public DefaultCardExecutionContextProvider(IPlayerManager playerManager, IEnemyManager enemyManager)
        {
            this.playerManager = playerManager;
            this.enemyManager = enemyManager;
        }

        /// <summary>
        /// 카드 실행에 필요한 시전자와 대상 정보를 포함한 컨텍스트를 생성합니다.
        /// </summary>
        /// <param name="card">실행할 스킬 카드</param>
        /// <returns>카드 실행 컨텍스트 객체</returns>
        public ICardExecutionContext CreateContext(ISkillCard card)
        {
            var source = playerManager?.GetPlayer();     // 시전자 (플레이어)
            var target = enemyManager?.GetEnemy();       // 대상자 (적, 단일 대상)

            if (source == null || target == null)
            {
                UnityEngine.Debug.LogWarning("[DefaultCardExecutionContextProvider] 시전자 또는 대상자 정보가 누락되었습니다.");
            }

            return new DefaultCardExecutionContext(card, source, target);
        }
    }
}
