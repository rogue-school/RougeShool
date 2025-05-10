using UnityEngine;
using Game.Data;
using Game.Enemy;
using Game.Managers;

namespace Game.Managers
{
    /// <summary>
    /// 스테이지 데이터를 기반으로 적을 순차적으로 소환하는 매니저입니다.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("스폰 대상 매니저")]
        [SerializeField] private EnemySpawnerManager enemySpawner;

        [Header("스테이지 데이터")]
        [SerializeField] private StageData currentStage;

        private int currentEnemyIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            AutoBindSpawner();
        }

        private void Start()
        {
            SpawnNextEnemy();
        }

        private void AutoBindSpawner()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawnerManager>();
                if (enemySpawner != null)
                    Debug.Log("[StageManager] EnemySpawner 자동 연결 완료");
                else
                    Debug.LogError("[StageManager] EnemySpawner를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 다음 적을 소환하고 전투 준비를 합니다.
        /// </summary>
        public void SpawnNextEnemy()
        {
            if (currentStage == null || currentStage.enemies == null || currentStage.enemies.Count == 0)
            {
                Debug.LogError("[StageManager] 스테이지 정보가 없습니다.");
                return;
            }

            if (currentEnemyIndex >= currentStage.enemies.Count)
            {
                Debug.Log("[StageManager] 모든 적 처치 완료");
                return;
            }

            var data = currentStage.enemies[currentEnemyIndex++];
            if (data?.prefab == null)
            {
                Debug.LogError($"[StageManager] 적 데이터 또는 프리팹이 null입니다 (Index: {currentEnemyIndex - 1})");
                return;
            }

            var enemy = enemySpawner.SpawnEnemy(data);
            if (enemy == null)
            {
                Debug.LogError("[StageManager] 적 생성 실패");
                return;
            }

            EnemyManager.Instance?.SetEnemy(enemy);
            EnemyHandManager.Instance.Initialize(enemy);
            EnemyHandManager.Instance.GenerateInitialHand();

            Debug.Log($"[StageManager] 적 소환 완료: {data.displayName}");
        }

        public StageData GetCurrentStage() => currentStage;
    }
}
