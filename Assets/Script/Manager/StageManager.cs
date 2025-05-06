using UnityEngine;
using Game.Enemy;
using Game.Slots;

namespace Game.Managers
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private EnemySpawnerManager enemySpawner;
        [SerializeField] private EnemyCharacterData[] currentStageEnemies;

        private void Awake()
        {
            AutoBindSpawner();
            LoadCurrentStageData();
            SpawnEnemies();
        }

        private void AutoBindSpawner()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<EnemySpawnerManager>();
                Debug.Log("[StageManager] EnemySpawner 자동 연결 완료");
            }
        }

        private void LoadCurrentStageData()
        {
            if (currentStageEnemies == null || currentStageEnemies.Length == 0)
            {
                Debug.LogWarning("[StageManager] currentStageEnemies가 비어 있습니다. 기본 데이터 적용 필요.");
            }
        }

        private void SpawnEnemies()
        {
            if (enemySpawner == null || currentStageEnemies == null || currentStageEnemies.Length == 0) return;

            // 현재 구조에선 하나의 적만 등장하므로 첫 번째 적만 소환
            enemySpawner.SpawnEnemy(currentStageEnemies[0]);

            Debug.Log($"[StageManager] 적 캐릭터 생성 완료: {currentStageEnemies[0].characterName}");
        }

        public EnemyCharacterData[] GetEnemies() => currentStageEnemies;
    }
}
