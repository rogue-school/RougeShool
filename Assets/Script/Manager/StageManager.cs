using UnityEngine;
using Game.Characters;
using Game.Managers;
using Game.Battle;

namespace Game.Stage
{
    public class StageManager : MonoBehaviour
    {
        [Header("스테이지 적 프리팹 목록")]
        [SerializeField] private GameObject[] enemyPrefabs;

        [Header("연동 매니저")]
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private BattleInitializer battleInitializer;
        [SerializeField] private BattleTurnManager turnManager;

        [Header("플레이어 참조")]
        [SerializeField] private PlayerCharacter playerUnit;

        private int currentStage = 0;
        private EnemyCharacter currentEnemy;

        private void Start()
        {
            SpawnNextEnemy();
        }

        public void SpawnNextEnemy()
        {
            if (currentStage >= enemyPrefabs.Length)
            {
                Debug.Log("[StageManager] 모든 스테이지를 완료했습니다!");
                return;
            }

            currentEnemy = enemySpawner.SpawnEnemy(enemyPrefabs[currentStage]);
            currentStage++;

            battleInitializer.Player = playerUnit;
            battleInitializer.Enemy = currentEnemy;

            battleInitializer.playerCardUI.Initialize(playerUnit.characterData, playerUnit);
            battleInitializer.enemyCardUI.Initialize(currentEnemy.characterData, currentEnemy);

            turnManager.StartNewTurn();
        }

        public void OnEnemyDefeated()
        {
            if (currentEnemy != null && currentEnemy.GetCurrentHP() <= 0)
            {
                Debug.Log("[StageManager] 적을 처치했습니다. 다음 적을 소환합니다.");
                SpawnNextEnemy();
            }
        }
    }
}
