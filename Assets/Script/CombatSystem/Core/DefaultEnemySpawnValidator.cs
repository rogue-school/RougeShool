using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// 적 스폰이 가능한지 여부를 판단하는 기본 구현체입니다.
    /// 현재 필드에 적이 없거나 사망한 경우에만 새로운 적 스폰이 가능합니다.
    /// </summary>
    public class DefaultEnemySpawnValidator : IEnemySpawnValidator
    {
        #region Fields

        private readonly IEnemyManager enemyManager;

        #endregion

        #region Constructor

        /// <summary>
        /// 기본 적 스폰 검증자를 생성합니다.
        /// </summary>
        /// <param name="enemyManager">적 매니저</param>
        public DefaultEnemySpawnValidator(IEnemyManager enemyManager)
        {
            this.enemyManager = enemyManager;
        }

        #endregion

        #region IEnemySpawnValidator Implementation

        /// <summary>
        /// 현재 적이 없거나 사망 상태이면 새로운 적을 스폰할 수 있습니다.
        /// </summary>
        /// <returns>적 스폰 가능 여부</returns>
        public bool CanSpawnEnemy()
        {
            var enemy = enemyManager?.GetEnemy();
            return enemy == null || enemy.IsDead();
        }

        #endregion
    }
}
