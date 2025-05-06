using UnityEngine;
using System.Collections.Generic;
using Game.Slots;
using Game.Characters;
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

        /// <summary>
        /// 적 캐릭터를 지정된 위치에 소환합니다 (현재 구조상 하나의 위치만 존재).
        /// </summary>
        public void SpawnEnemy(EnemyCharacterData data)
        {
            // 고정된 적 슬롯 위치를 가져옴
            var slot = SlotRegistry.Instance.GetCharacterSlot(CharacterSlotPosition.ENEMY_POSITION);

            if (slot == null)
            {
                Debug.LogError("[EnemySpawnerManager] ENEMY_POSITION 슬롯을 찾지 못했습니다.");
                return;
            }

            // 슬롯 위치에 적 프리팹 생성
            GameObject instance = Instantiate(enemyPrefab, slot.transform.position, Quaternion.identity);
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
