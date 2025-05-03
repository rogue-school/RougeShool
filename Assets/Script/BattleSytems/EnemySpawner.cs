using UnityEngine;
using Game.Characters;
using Game.Cards;

namespace Game.Managers
{
    /// <summary>
    /// 적을 현재 스테이지에 맞게 소환하는 스폰 매니저입니다.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;

        /// <summary>
        /// 적 프리팹을 스폰하여 전투에 등장시킵니다.
        /// </summary>
        public EnemyCharacter SpawnEnemy(GameObject enemyPrefab)
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] 소환할 적 프리팹이 없습니다.");
                return null;
            }

            GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            EnemyCharacter enemyUnit = enemyGO.GetComponent<EnemyCharacter>();

            return enemyUnit;
        }
    }
}
