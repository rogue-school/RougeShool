using UnityEngine;
using Game.CombatSystem.Stage;
using Game.IManager;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Core;
using Zenject;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 현재 스테이지를 관리하고, 적 캐릭터 데이터를 기반으로 적을 생성합니다.
    /// 핸드 생성은 EnemyHandInitializer에서 수행되므로 여기서는 캐릭터만 등록합니다.
    /// </summary>
    public class StageManager : MonoBehaviour, IStageManager
    {
        [Header("스테이지 데이터")]
        [SerializeField] private StageData currentStage;

        private int currentEnemyIndex = 0;
        private bool isSpawning = false;

        [Inject] private IEnemySpawnerManager spawnerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IEnemySpawnValidator spawnValidator;
        [Inject] private ICharacterDeathListener deathListener;

        public void SpawnNextEnemy()
        {
            if (isSpawning)
            {
                Debug.LogWarning("[StageManager] 중복 스폰 방지");
                return;
            }

            if (!spawnValidator.CanSpawnEnemy())
            {
                Debug.LogWarning("[StageManager] 스폰 조건 미충족");
                return;
            }

            if (enemyManager.GetEnemy() != null)
            {
                Debug.LogWarning("[StageManager] 이미 적이 존재합니다.");
                return;
            }

            if (!TryGetNextEnemyData(out var data))
            {
                Debug.LogWarning("[StageManager] 다음 적 데이터를 가져올 수 없습니다.");
                return;
            }

            isSpawning = true;

            var result = spawnerManager.SpawnEnemy(data);
            if (result?.Enemy == null)
            {
                Debug.LogError("[StageManager] 적 생성 실패");
                isSpawning = false;
                return;
            }

            RegisterEnemy(result.Enemy);
            currentEnemyIndex++;

            isSpawning = false;
            Debug.Log("[StageManager] 적 생성 완료");
        }

        private void RegisterEnemy(IEnemyCharacter enemy)
        {
            enemyManager.RegisterEnemy(enemy);
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
