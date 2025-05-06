using UnityEngine;
using System.Collections.Generic;
using Game.Characters;
using Game.Enemy;
using Game.Cards;
using Game.Battle;

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
            if (enemySpawner == null || currentStageEnemies == null) return;

            BattleSlotPosition[] defaultSlots = {
                BattleSlotPosition.FIRST,
                BattleSlotPosition.SECOND
            };

            for (int i = 0; i < currentStageEnemies.Length; i++)
            {
                var position = (i < defaultSlots.Length) ? defaultSlots[i] : BattleSlotPosition.FIRST;
                enemySpawner.SpawnEnemy(currentStageEnemies[i], position);
            }

            Debug.Log($"[StageManager] {currentStageEnemies.Length}명의 적을 생성했습니다.");
        }

        public EnemyCharacterData[] GetEnemies() => currentStageEnemies;
    }
}
