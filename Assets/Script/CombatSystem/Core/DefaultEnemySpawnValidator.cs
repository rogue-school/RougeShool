using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Core
{
    public class DefaultEnemySpawnValidator : IEnemySpawnValidator
    {
        private readonly IEnemyManager enemyManager;

        public DefaultEnemySpawnValidator(IEnemyManager enemyManager)
        {
            this.enemyManager = enemyManager;
        }

        public bool CanSpawnEnemy()
        {
            var enemy = enemyManager?.GetEnemy();
            return enemy == null || enemy.IsDead();
        }
    }
}
