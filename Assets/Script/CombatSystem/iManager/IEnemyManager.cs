using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.IManager
{
    public interface IEnemyManager
    {
        void RegisterEnemy(IEnemyCharacter enemy);
        IEnemyCharacter GetEnemy();
        IEnemyCharacter GetCurrentEnemy();
        bool HasEnemy();
        void ClearEnemy();

        IEnemyHandManager GetEnemyHandManager();
        void Reset();
    }
}
