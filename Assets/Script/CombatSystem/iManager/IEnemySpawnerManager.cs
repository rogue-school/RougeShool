using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CombatSystem.Utility;

namespace Game.IManager
{
    public interface IEnemySpawnerManager
    {
        void SpawnInitialEnemy();
        EnemySpawnResult SpawnEnemy(EnemyCharacterData data);
        List<EnemyCharacter> GetAllEnemies();
    }
}
