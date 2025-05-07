using UnityEngine;
using System.Collections.Generic;
using Game.Slots;
using Game.Data;
using Game.Managers;

namespace Game.Enemy
{
    /// <summary>
    /// 전투 시작 시 EnemyCharacter를 지정된 슬롯에 배치하고 초기화합니다.
    /// </summary>
    public class EnemySpawnerManager : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;

        private List<EnemyCharacter> spawnedEnemies = new List<EnemyCharacter>();

        public void SpawnEnemy(EnemyCharacterData data)
        {
            var slot = SlotRegistry.Instance.GetCharacterSlot(SlotOwner.ENEMY);

            if (slot == null)
            {
                Debug.LogError("[EnemySpawnerManager] ENEMY 슬롯을 찾지 못했습니다.");
                return;
            }

            GameObject instance = Instantiate(enemyPrefab, ((MonoBehaviour)slot).transform.position, Quaternion.identity);
            EnemyCharacter enemy = instance.GetComponent<EnemyCharacter>();

            if (enemy == null)
            {
                Debug.LogError("[EnemySpawnerManager] EnemyCharacter 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            enemy.Initialize(data.maxHP);
            spawnedEnemies.Add(enemy);

            Debug.Log("[EnemySpawnerManager] 적 캐릭터가 슬롯에 배치되었습니다.");
        }

        public List<EnemyCharacter> GetAllEnemies() => spawnedEnemies;
    }
}
