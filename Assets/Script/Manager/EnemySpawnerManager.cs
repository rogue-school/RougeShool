using UnityEngine;
using Game.Characters;
using Game.Enemy;
using System;
using System.Collections.Generic;
using Game.Cards;

namespace Game.Managers
{
    /// <summary>
    /// 스테이지에서 적 유닛을 생성하는 매니저입니다.
    /// 자동으로 스폰 위치를 참조하고, 적 프리팹을 로드하여 초기화합니다.
    /// </summary>
    public class EnemySpawnerManager : MonoBehaviour
    {
        [Header("스폰 위치들")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("적 프리팹 (Resources 경로)")]
        [SerializeField] private string enemyPrefabPath = "Prefabs/Enemies/EnemyCharacter";

        private int spawnIndex = 0;

        private void Awake()
        {
            AutoBindSpawnPoints();
        }

        /// <summary>
        /// 씬 내에서 "EnemySpawnPoint" 태그를 가진 오브젝트들을 자동으로 수집합니다.
        /// </summary>
        private void AutoBindSpawnPoints()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                List<Transform> foundPoints = new List<Transform>();
                foreach (var obj in GameObject.FindGameObjectsWithTag("EnemySpawnPoint"))
                {
                    foundPoints.Add(obj.transform);
                }

                spawnPoints = foundPoints.ToArray();
                Array.Sort(spawnPoints, (a, b) => a.name.CompareTo(b.name));
                Debug.Log($"[EnemySpawnerManager] 스폰 위치 {spawnPoints.Length}개 자동 참조 완료");
            }
        }

        /// <summary>
        /// 지정된 데이터를 기반으로 적 캐릭터를 스폰합니다.
        /// </summary>
        public void SpawnEnemy(EnemyCharacterData data)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[EnemySpawnerManager] 스폰 위치가 존재하지 않습니다.");
                return;
            }

            GameObject enemyPrefab = Resources.Load<GameObject>(enemyPrefabPath);

            if (enemyPrefab == null)
            {
                Debug.LogError($"[EnemySpawnerManager] 프리팹 경로가 잘못되었습니다: {enemyPrefabPath}");
                return;
            }

            Transform spawnPoint = spawnPoints[Mathf.Clamp(spawnIndex, 0, spawnPoints.Length - 1)];
            GameObject spawned = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

            EnemyCharacter character = spawned.GetComponent<EnemyCharacter>();
            if (character != null)
            {
                Debug.Log($"[EnemySpawnerManager] 적 캐릭터 생성 완료: {data.characterName}");
            }

            spawnIndex++;
        }
    }
}
