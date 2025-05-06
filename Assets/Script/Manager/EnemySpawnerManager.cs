using UnityEngine;
using System.Collections.Generic;
using Game.Slots;
using Game.Battle;
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
        /// 지정된 위치에 적을 생성하고 초기화합니다.
        /// </summary>
        public void SpawnEnemy(EnemyCharacterData data, BattleSlotPosition battleSlotPosition)
        {
            // 지정된 위치에 해당하는 슬롯을 찾는다
            var slot = SlotRegistry.Instance.GetSlot(
                SlotOwner.Enemy,
                SlotRole.CharacterSpawn,
                battleSlotPosition
            );

            if (slot == null)
            {
                Debug.LogError($"[EnemySpawnerManager] Enemy {battleSlotPosition} 슬롯을 찾지 못했습니다.");
                return;
            }

            // 슬롯 위치에 적 프리팹 생성
            GameObject instance = Instantiate(enemyPrefab, slot.transform.position, Quaternion.identity);
            EnemyCharacter enemy = instance.GetComponent<EnemyCharacter>();

            if (enemy == null)
            {
                Debug.LogError("[EnemySpawnerManager] 생성된 오브젝트에 EnemyCharacter 컴포넌트가 없습니다.");
                return;
            }

            enemy.Initialize(data.maxHP);
            spawnedEnemies.Add(enemy);

            Debug.Log($"[EnemySpawnerManager] 적 캐릭터가 {battleSlotPosition} 슬롯에 배치되었습니다.");
        }

        public List<EnemyCharacter> GetAllEnemies() => spawnedEnemies;
    }
}
