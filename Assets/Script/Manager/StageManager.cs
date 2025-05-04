using UnityEngine;
using System.Collections.Generic;
using Game.Characters;
using Game.Enemy;
using Game.Cards;

namespace Game.Managers
{
    /// <summary>
    /// 현재 스테이지에 등장할 적 유닛을 관리하고 EnemySpawner에 전달합니다.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private EnemySpawnerManager enemySpawner;

        [Header("스테이지 데이터")]
        [SerializeField] private EnemyCharacterData[] currentStageEnemies;

        private void Awake()
        {
            AutoBindSpawner();
            LoadCurrentStageData();
            SpawnEnemies();
        }

        /// <summary>
        /// EnemySpawner를 자동으로 참조합니다.
        /// </summary>
        private void AutoBindSpawner()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<EnemySpawnerManager>();
                Debug.Log("[StageManager] EnemySpawner 자동 연결 완료");
            }
        }

        /// <summary>
        /// 현재 스테이지의 적 캐릭터 데이터를 불러옵니다.
        /// 향후 확장 시 외부 JSON 또는 Resources에서 불러올 수 있음.
        /// </summary>
        private void LoadCurrentStageData()
        {
            if (currentStageEnemies == null || currentStageEnemies.Length == 0)
            {
                Debug.LogWarning("[StageManager] currentStageEnemies가 비어 있습니다. 기본 데이터 적용 필요.");
                // 예시: Resources에서 로드
                // currentStageEnemies = Resources.LoadAll<EnemyCharacterData>("Stage1");
            }
        }

        /// <summary>
        /// 현재 스테이지에 등장할 적들을 생성합니다.
        /// </summary>
        private void SpawnEnemies()
        {
            if (enemySpawner == null || currentStageEnemies == null) return;

            foreach (var enemyData in currentStageEnemies)
            {
                enemySpawner.SpawnEnemy(enemyData);
            }

            Debug.Log($"[StageManager] {currentStageEnemies.Length}명의 적을 생성했습니다.");
        }

        /// <summary>
        /// 현재 스테이지에 설정된 적 데이터 반환 (외부 접근용)
        /// </summary>
        public EnemyCharacterData[] GetEnemies()
        {
            return currentStageEnemies;
        }
    }
}
