using UnityEngine;
using System.Collections.Generic;
using Game.Slots;
using Game.Data;
using Game.Managers;

namespace Game.Enemy
{
    /// <summary>
    /// 적 캐릭터를 슬롯에 배치하는 스포너 매니저입니다.
    /// </summary>
    public class EnemySpawnerManager : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;

        private List<EnemyCharacter> spawnedEnemies = new List<EnemyCharacter>();

        public EnemyCharacter SpawnEnemy(EnemyCharacterData data)
        {
            var slot = SlotRegistry.Instance.GetCharacterSlot(SlotOwner.ENEMY);

            if (slot == null)
            {
                Debug.LogError("[EnemySpawnerManager] ENEMY 슬롯을 찾지 못했습니다.");
                return null;
            }

            GameObject instance = Instantiate(enemyPrefab, ((MonoBehaviour)slot).transform.position, Quaternion.identity);
            EnemyCharacter enemy = instance.GetComponent<EnemyCharacter>();

            if (enemy == null)
            {
                Debug.LogError("[EnemySpawnerManager] EnemyCharacter 컴포넌트를 찾을 수 없습니다.");
                return null;
            }

            enemy.SetCharacterData(data);
            slot.SetCharacter(enemy);
            spawnedEnemies.Add(enemy);

            Debug.Log($"[EnemySpawnerManager] 적 캐릭터 배치 완료: {data.displayName}");
            return enemy;
        }

        public List<EnemyCharacter> GetAllEnemies() => spawnedEnemies;
    }
}
