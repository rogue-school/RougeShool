using UnityEngine;
using Game.Data;
using Game.Enemy;

namespace Game.Managers
{
    /// <summary>
    /// 스테이지 데이터를 기반으로 적을 소환하는 매니저
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("스폰 대상 매니저")]
        [SerializeField] private EnemySpawnerManager enemySpawner;

        [Header("스테이지 데이터")]
        [SerializeField] private StageData currentStage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            AutoBindSpawner();
            SpawnEnemies();
        }

        private void AutoBindSpawner()
        {
            if (enemySpawner == null)
            {
                enemySpawner = Object.FindFirstObjectByType<EnemySpawnerManager>();
                if (enemySpawner != null)
                    Debug.Log("[StageManager] EnemySpawner 자동 연결 완료");
                else
                    Debug.LogError("[StageManager] EnemySpawner를 찾을 수 없습니다.");
            }
        }

        private void SpawnEnemies()
        {
            if (currentStage == null || currentStage.enemies == null || currentStage.enemies.Length == 0)
            {
                Debug.LogWarning("[StageManager] 적 데이터가 없습니다.");
                return;
            }

            EnemyCharacter spawnedEnemy = enemySpawner.SpawnEnemy(currentStage.enemies[0]);

            if (spawnedEnemy != null && EnemyHandManager.Instance != null)
            {
                EnemyHandManager.Instance.Initialize(spawnedEnemy);
                EnemyHandManager.Instance.GenerateInitialHand();
            }

            Debug.Log($"[StageManager] 스테이지 적 생성 및 핸드 카드 초기화 완료: {currentStage.enemies[0].displayName}");
        }

        public StageData GetCurrentStage() => currentStage;
    }
}
