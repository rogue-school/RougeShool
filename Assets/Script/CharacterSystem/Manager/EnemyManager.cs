using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.IManager;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IEnemyManager
{
    private IEnemyCharacter currentEnemy;
    private IEnemyHandManager enemyHandManager;

    public void RegisterEnemy(IEnemyCharacter enemy)
    {
        currentEnemy = enemy;
    }

    public IEnemyCharacter GetEnemy() => currentEnemy;

    public IEnemyCharacter GetCurrentEnemy() => currentEnemy;

    public bool HasEnemy() => currentEnemy != null;

    public void ClearEnemy()
    {
        currentEnemy = null;
        enemyHandManager = null;
    }

    public void SetEnemyHandManager(IEnemyHandManager handManager)
    {
        enemyHandManager = handManager;
    }

    public IEnemyHandManager GetEnemyHandManager()
    {
        return enemyHandManager;
    }
    public void Reset()
    {
        // 적 상태 초기화 로직 구현
        Debug.Log("[EnemyManager] Reset");
    }
}
