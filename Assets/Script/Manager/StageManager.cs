using UnityEngine;
using System.Collections.Generic;
using Game.Characters;
using Game.Enemy;
using Game.Cards;
using Game.Battle;

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
            if (enemySpawner == null || currentStageEnemies == null) return;

            SlotPosition[] defaultSlots = {
                SlotPosition.FRONT,
                SlotPosition.BACK,
                SlotPosition.SUPPORT
            };

            for (int i = 0; i < currentStageEnemies.Length; i++)
            {
                var position = (i < defaultSlots.Length) ? defaultSlots[i] : SlotPosition.UNKNOWN;
                enemySpawner.SpawnEnemy(currentStageEnemies[i], position);
            }

            Debug.Log($"[StageManager] {currentStageEnemies.Length}명의 적을 생성했습니다.");
        }

        public EnemyCharacterData[] GetEnemies()
        {
            return currentStageEnemies;
        }
    }
}

