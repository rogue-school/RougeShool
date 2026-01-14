using Game.CharacterSystem.Core;

namespace Game.CombatSystem.Utility
{
    public class EnemySpawnResult
    {
        public EnemyCharacter Enemy { get; }
        public bool IsNewlySpawned { get; }

        public EnemySpawnResult(EnemyCharacter enemy, bool isNewlySpawned)
        {
            Enemy = enemy;
            IsNewlySpawned = isNewlySpawned;
        }
    }
}
