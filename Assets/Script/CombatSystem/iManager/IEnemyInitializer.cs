using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;

namespace Game.IManager
{
    public interface IEnemyInitializer
    {
        void SetupWithData(EnemyCharacterData data);
        IEnemyCharacter GetSpawnedEnemy();
    }
}