using UnityEngine;
using Game.CombatSystem.Stage;
using Game.IManager;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using Game.CharacterSystem.Core;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Manager
{
    public class StageManager : MonoBehaviour, IStageManager
    {
        [Header("스테이지 데이터")]
        [SerializeField] private StageData currentStage;

        private int currentEnemyIndex = 0;

        private IEnemySpawnerManager spawnerManager;
        private IEnemyManager enemyManager;
        private IEnemyHandManager handManager;
        private IEnemySpawnValidator spawnValidator;
        private ICharacterDeathListener deathListener;
        private ISlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;

        public void Inject(
            IEnemySpawnerManager spawner,
            IEnemyManager enemyManager,
            IEnemyHandManager handManager,
            IEnemySpawnValidator spawnValidator,
            ICharacterDeathListener deathListener,
            ISlotRegistry slotRegistry,
            ISkillCardFactory cardFactory)
        {
            this.spawnerManager = spawner;
            this.enemyManager = enemyManager;
            this.handManager = handManager;
            this.spawnValidator = spawnValidator;
            this.deathListener = deathListener;
            this.slotRegistry = slotRegistry;
            this.cardFactory = cardFactory;
        }

        public void SpawnNextEnemy()
        {
            Debug.Log($"[StageManager] SpawnNextEnemy 호출 - 현재 인덱스: {currentEnemyIndex}");

            if (!spawnValidator.CanSpawnEnemy())
            {
                Debug.LogWarning("[StageManager] 현재 적이 살아있으므로 새 적을 소환하지 않습니다.");
                return;
            }

            if (!TryGetNextEnemyData(out var enemyData))
            {
                Debug.LogWarning("[StageManager] 다음 적 데이터를 가져올 수 없습니다.");
                return;
            }

            var result = spawnerManager.SpawnEnemy(enemyData);
            if (result == null || result.Enemy == null)
            {
                Debug.LogError("[StageManager] 적 생성 실패 또는 null 반환");
                return;
            }

            if (!result.IsNewlySpawned)
            {
                Debug.LogWarning("[StageManager] 새 적이 아닌 기존 적입니다. 인덱스 증가 생략");
                return;
            }

            RegisterEnemy(result.Enemy);
            SetupEnemyHand(result.Enemy);
            currentEnemyIndex++;

            Debug.Log($"[StageManager] 적 소환 완료: {result.Enemy.GetCharacterName()} (index: {currentEnemyIndex})");
        }

        private void SetupEnemyHand(IEnemyCharacter enemy)
        {
            if (handManager == null || slotRegistry == null || cardFactory == null)
            {
                Debug.LogWarning("[StageManager] HandManager 초기화 인자 누락");
                return;
            }

            handManager.Initialize(enemy, slotRegistry, cardFactory);
            handManager.GenerateInitialHand();

            Debug.Log("[StageManager] EnemyHandManager 초기화 및 초기 핸드 생성 완료");
        }

        private void RegisterEnemy(IEnemyCharacter enemy)
        {
            if (enemyManager == null)
            {
                Debug.LogWarning("[StageManager] enemyManager가 주입되지 않았습니다. Register 생략");
                return;
            }

            enemyManager.RegisterEnemy(enemy);

            if (enemy is EnemyCharacter concreteEnemy && deathListener != null)
            {
                concreteEnemy.SetDeathListener(deathListener);
            }

            Debug.Log($"[StageManager] 적 등록 완료: {enemy.GetCharacterName()}");
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
            if (data == null || data.Prefab == null)
            {
                Debug.LogError($"[StageManager] Enemy 데이터가 유효하지 않습니다. Index: {currentEnemyIndex}");
                return false;
            }

            return true;
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

        public EnemyCharacterData PeekNextEnemyData()
        {
            if (currentStage == null || currentStage.enemies == null || currentEnemyIndex >= currentStage.enemies.Count)
            {
                Debug.LogWarning("[StageManager] PeekNextEnemyData: 더 이상 남은 적이 없습니다.");
                return null;
            }

            return currentStage.enemies[currentEnemyIndex];
        }
    }
}
