using UnityEngine;
using Game.Enemy;
using Game.Data;

namespace Game.Initialization
{
    public class EnemyInitializer : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private EnemyCharacterData defaultData;

        public void SpawnEnemy(EnemyCharacterData data)
        {
            var enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemy.SetCharacterData(data);
        }

        public void Setup()
        {
            if (defaultData != null)
                SpawnEnemy(defaultData);
            else
                Debug.LogWarning("[EnemyInitializer] 기본 데이터가 설정되지 않았습니다.");
        }
    }
}
