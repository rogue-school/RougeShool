using UnityEngine;
using Game.CombatSystem.Stage;
using Game.IManager;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 스테이지 데이터를 기반으로 적을 순차적으로 소환하는 매니저입니다.
    /// </summary>
    public class StageManager : MonoBehaviour, IStageManager
    {
        [Header("스테이지 데이터")]
        [SerializeField] private StageData currentStage;

        private int currentEnemyIndex = 0;

        private IEnemySpawnerManager spawnerManager;
        private IEnemyManager enemyManager;
        private IEnemyHandManager handManager;

        public void Inject(
            IEnemySpawnerManager spawner,
            IEnemyManager enemyManager,
            IEnemyHandManager handManager)
        {
            this.spawnerManager = spawner;
            this.enemyManager = enemyManager;
            this.handManager = handManager;

            //Debug.Log("[StageManager] 의존성 주입 완료");
        }

        public void SpawnNextEnemy()
        {
            Debug.Log($"[StageManager] SpawnNextEnemy 호출 - 현재 인덱스: {currentEnemyIndex}");

            if (!TryGetNextEnemyData(out var enemyData))
            {
                Debug.LogWarning("[StageManager] 다음 적 데이터를 가져올 수 없습니다.");
                return;
            }

            if (!ValidateSpawner())
            {
                Debug.LogError("[StageManager] SpawnerManager가 주입되지 않았습니다.");
                return;
            }

            var enemy = spawnerManager.SpawnEnemy(enemyData);
            if (enemy == null)
            {
                Debug.LogError($"[StageManager] SpawnEnemy 실패 - 적 생성 실패: {enemyData.displayName}");
                return; // **핵심: 여기서 바로 중단**
            }

            RegisterEnemy(enemy);
            SetupEnemyHand(enemy);
            Debug.Log($"[StageManager] 적 소환 완료: {enemyData.displayName} (Index: {currentEnemyIndex})");

            currentEnemyIndex++; // **정상 생성 후에만 인덱스 증가**
        }



        private bool TryGetNextEnemyData(out EnemyCharacterData data)
        {
            data = null;

            if (currentStage == null || currentStage.enemies == null || currentStage.enemies.Count == 0)
            {
                Debug.LogError("[StageManager] 스테이지 또는 적 리스트가 비어 있습니다.");
                return false;
            }

            if (currentEnemyIndex >= currentStage.enemies.Count)
            {
                Debug.Log("[StageManager] 모든 적이 소환되었습니다.");
                return false;
            }

            data = currentStage.enemies[currentEnemyIndex];
            if (data == null)
            {
                Debug.LogError($"[StageManager] Enemy 데이터가 null입니다. Index: {currentEnemyIndex}");
                return false;
            }

            if (data.prefab == null)
            {
                Debug.LogError($"[StageManager] Enemy 프리팹이 null입니다. Name: {data.displayName}");
                return false;
            }

            return true;
        }

        private bool ValidateSpawner()
        {
            if (spawnerManager == null)
            {
                Debug.LogError("[StageManager] spawnerManager가 주입되지 않았습니다.");
                return false;
            }
            return true;
        }

        private void RegisterEnemy(IEnemyCharacter enemy)
        {
            if (enemyManager == null)
            {
                Debug.LogWarning("[StageManager] enemyManager가 주입되지 않았습니다. Register 생략");
                return;
            }

            enemyManager.RegisterEnemy(enemy);
            Debug.Log($"[StageManager] 적 등록 완료: {enemy.GetCharacterName()}");
        }

        private void SetupEnemyHand(IEnemyCharacter enemy)
        {
            if (handManager == null)
            {
                Debug.LogWarning("[StageManager] handManager가 주입되지 않았습니다. 초기화 생략");
                return;
            }

            handManager.Initialize(enemy);
            handManager.GenerateInitialHand();
            Debug.Log("[StageManager] EnemyHandManager 초기화 및 초기 핸드 생성 완료");
        }

        public StageData GetCurrentStage() => currentStage;

        public bool HasNextEnemy()
        {
            bool hasNext = currentStage != null &&
                           currentStage.enemies != null &&
                           currentEnemyIndex < currentStage.enemies.Count;

            Debug.Log($"[StageManager] HasNextEnemy? → {hasNext}");
            return hasNext;
        }
    }
}
