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
using Zenject;

namespace Game.CombatSystem.Manager
{
    public class StageManager : MonoBehaviour, IStageManager
    {
        [Header("스테이지 데이터")]
        [SerializeField] private StageData currentStage;

        [Inject] private IEnemySpawnerManager spawnerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IEnemyHandManager handManager;
        [Inject] private IEnemySpawnValidator spawnValidator;
        [Inject] private ICharacterDeathListener deathListener;
        [Inject] private ISlotRegistry slotRegistry;
        [Inject] private ISkillCardFactory cardFactory;

        private int currentEnemyIndex = 0;

        public void SpawnNextEnemy()
        {
            if (!spawnValidator.CanSpawnEnemy())
                return;

            if (!TryGetNextEnemyData(out var data))
                return;

            var result = spawnerManager.SpawnEnemy(data);
            if (result == null || result.Enemy == null || !result.IsNewlySpawned)
                return;

            RegisterEnemy(result.Enemy);
            SetupEnemyHand(result.Enemy);
            currentEnemyIndex++;
        }

        private void SetupEnemyHand(IEnemyCharacter enemy)
        {
            handManager?.Initialize(enemy, slotRegistry, cardFactory);
            handManager?.GenerateInitialHand();
        }

        private void RegisterEnemy(IEnemyCharacter enemy)
        {
            enemyManager?.RegisterEnemy(enemy);
            if (enemy is EnemyCharacter concrete && deathListener != null)
                concrete.SetDeathListener(deathListener);
        }

        private bool TryGetNextEnemyData(out EnemyCharacterData data)
        {
            data = null;
            if (currentStage == null || currentStage.enemies == null || currentEnemyIndex >= currentStage.enemies.Count)
                return false;

            data = currentStage.enemies[currentEnemyIndex];
            return data != null && data.Prefab != null;
        }

        public StageData GetCurrentStage() => currentStage;
        public bool HasNextEnemy() => currentStage != null && currentEnemyIndex < currentStage.enemies.Count;
        public EnemyCharacterData PeekNextEnemyData() => HasNextEnemy() ? currentStage.enemies[currentEnemyIndex] : null;
    }
}
