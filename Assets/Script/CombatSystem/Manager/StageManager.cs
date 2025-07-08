using UnityEngine;
using Game.CombatSystem.Stage;
using Game.IManager;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Core;
using Zenject;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Manager
{
    /// <summary>
    /// 현재 스테이지의 적 캐릭터를 관리하고 생성합니다.
    /// 핸드 등록은 EnemyHandInitializer 등 다른 시스템에서 처리합니다.
    /// </summary>
    public class StageManager : MonoBehaviour, IStageManager
    {
        #region 인스펙터 필드

        [Header("스테이지 데이터")]
        [SerializeField] private StageData currentStage;

        #endregion

        #region 내부 상태

        private int currentEnemyIndex = 0;
        private bool isSpawning = false;

        #endregion

        #region 의존성 주입

        [Inject] private IEnemySpawnerManager spawnerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private IEnemySpawnValidator spawnValidator;
        [Inject] private ICharacterDeathListener deathListener;

        #endregion

        #region 적 생성 흐름

        /// <summary>
        /// 다음 적을 생성하여 전투에 배치합니다. (코루틴 기반)
        /// </summary>
        public System.Collections.IEnumerator SpawnNextEnemyCoroutine()
        {
            if (isSpawning)
            {
                Debug.LogWarning("[StageManager] 중복 스폰 방지");
                yield break;
            }

            if (!spawnValidator.CanSpawnEnemy())
            {
                Debug.LogWarning("[StageManager] 스폰 조건 미충족");
                yield break;
            }

            if (enemyManager.GetEnemy() != null)
            {
                Debug.LogWarning("[StageManager] 이미 적이 존재합니다.");
                yield break;
            }

            if (!TryGetNextEnemyData(out var data))
            {
                Debug.LogWarning("[StageManager] 다음 적 데이터를 가져올 수 없습니다.");
                yield break;
            }

            isSpawning = true;
            EnemySpawnResult result = null;
            yield return spawnerManager.SpawnEnemyWithAnimation(data, r => { result = r; });
            if (result?.Enemy == null)
            {
                Debug.LogError("[StageManager] 적 생성 실패");
                isSpawning = false;
                yield break;
            }
            RegisterEnemy(result.Enemy);
            currentEnemyIndex++;
            isSpawning = false;
            Debug.Log("[StageManager] 적 생성 완료");
        }

        /// <summary>
        /// 기존 API 호환성을 위해 StartCoroutine으로 코루틴을 호출
        /// </summary>
        public void SpawnNextEnemy()
        {
            StartCoroutine(SpawnNextEnemyCoroutine());
        }

        /// <summary>
        /// 적 캐릭터를 시스템에 등록하고, 사망 리스너를 연결합니다.
        /// </summary>
        private void RegisterEnemy(IEnemyCharacter enemy)
        {
            enemyManager.RegisterEnemy(enemy);
            if (enemy is EnemyCharacter concrete && deathListener != null)
                concrete.SetDeathListener(deathListener);
        }

        /// <summary>
        /// 다음 적 데이터를 조회합니다.
        /// </summary>
        private bool TryGetNextEnemyData(out EnemyCharacterData data)
        {
            data = null;

            if (currentStage == null ||
                currentStage.enemies == null ||
                currentEnemyIndex >= currentStage.enemies.Count)
                return false;

            data = currentStage.enemies[currentEnemyIndex];
            return data != null && data.Prefab != null;
        }

        #endregion

        #region 스테이지 정보

        /// <inheritdoc />
        public StageData GetCurrentStage() => currentStage;

        /// <inheritdoc />
        public bool HasNextEnemy() =>
            currentStage != null && currentEnemyIndex < currentStage.enemies.Count;

        /// <inheritdoc />
        public EnemyCharacterData PeekNextEnemyData() =>
            HasNextEnemy() ? currentStage.enemies[currentEnemyIndex] : null;

        #endregion
    }
}
