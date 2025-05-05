using UnityEngine;
using Game.Characters;
using Game.Battle;
using Game.Enemy;
using System.Collections.Generic;
using Game.Cards;

namespace Game.Managers
{
    /// <summary>
    /// 적 캐릭터를 슬롯 기반으로 관리하는 매니저입니다.
    /// </summary>
    public class EnemySpawnerManager : MonoBehaviour
    {
        public static EnemySpawnerManager Instance { get; private set; }

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform[] spawnPoints;

        private readonly List<EnemyCharacter> enemies = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SpawnEnemy(EnemyCharacterData data, SlotPosition position)
        {
            var spawnIndex = (int)position;
            if (spawnIndex >= spawnPoints.Length)
            {
                Debug.LogWarning($"[EnemySpawnerManager] 유효하지 않은 슬롯 위치: {position}");
                return;
            }

            var spawned = Instantiate(enemyPrefab, spawnPoints[spawnIndex].position, Quaternion.identity);
            var enemy = spawned.GetComponent<EnemyCharacter>();
            if (enemy != null)
            {
                enemy.Initialize(data, position);
                enemies.Add(enemy);
            }
        }

        public EnemyCharacter GetEnemyBySlot(SlotPosition position)
        {
            foreach (var enemy in enemies)
            {
                if (enemy.SlotPosition == position)
                    return enemy;
            }

            Debug.LogWarning($"[EnemySpawnerManager] 슬롯 {position}에 해당하는 적이 없습니다.");
            return null;
        }
    }
}
