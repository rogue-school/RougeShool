using UnityEngine;
using System.Threading.Tasks;
using System;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Interface;
using Game.StageSystem.Data;
using Game.StageSystem.Interface;
using Zenject;
using Game.CoreSystem.Utility;

namespace Game.StageSystem.Manager
{
    /// <summary>
    /// 스테이지 진행을 관리하는 매니저입니다.
    /// 스테이지의 모든 적을 순차적으로 생성하고,
    /// 모든 적 처치 시 스테이지 완료(승리)를 처리합니다.
    /// </summary>
    public class StageManager : MonoBehaviour, IStageManager
    {
        #region 인스펙터 필드

        [System.Serializable]
        public class StageSettings
        {
            [Header("스테이지 데이터")]
            [Tooltip("현재 스테이지 데이터")]
            public StageData currentStage;

            [Space(5)]
            [Header("진행 설정")]
            [Tooltip("자동 스테이지 진행")]
            public bool autoProgress = true;

            [Tooltip("스테이지 전환 지연 시간 (초)")]
            [Range(0f, 5f)]
            public float transitionDelay = 1f;

        }


        [System.Serializable]
        public class DebugSettings
        {
            [Header("디버그 옵션")]
            [Tooltip("스테이지 정보 로깅")]
            public bool enableStageLogging = true;

            [Tooltip("적 상태 시각화")]
            public bool showEnemyStatus = false;

            [Tooltip("보상 정보 표시")]
            public bool showRewardInfo = false;

            [Tooltip("스테이지 진행 상태 표시")]
            public bool showProgressStatus = false;
        }

        [Header("🏰 스테이지 설정")]
        [SerializeField] private StageSettings stageSettings = new StageSettings();
        
        [Space(10)]
        [Header("🔧 디버그 설정")]
        [SerializeField] private DebugSettings debugSettings = new DebugSettings();

        #endregion

        #region 내부 상태

        private int currentEnemyIndex = 0;
        private bool isSpawning = false;
        private bool isStageCompleted = false;
        
        // 스테이지 진행 상태
        private StageProgressState progressState = StageProgressState.NotStarted;

        #endregion

        #region 이벤트

        /// <summary>적 처치 시 호출되는 이벤트</summary>
        public event Action<ICharacter> OnEnemyDefeated;
        
        /// <summary>스테이지 완료 시 호출되는 이벤트</summary>
        public event Action<StageData> OnStageCompleted;

        #endregion

        #region 의존성 주입 (최소화)

        // 핵심 의존성만 유지
        // EnemyManager는 런타임에 찾아서 사용

        #endregion

        #region 적 생성 흐름

        /// <summary>
        /// 다음 적을 생성하여 전투에 배치합니다. (async/await 기반)
        /// </summary>
        public async Task<bool> SpawnNextEnemyAsync()
        {
            if (isSpawning)
            {
                GameLogger.LogWarning("중복 스폰 방지", GameLogger.LogCategory.Combat);
                return false;
            }

            var enemyManager = FindFirstObjectByType<EnemyManager>();
            if (enemyManager?.GetEnemy() != null)
            {
                GameLogger.LogWarning("이미 적이 존재합니다", GameLogger.LogCategory.Combat);
                return false;
            }

            if (!TryGetNextEnemyData(out var data))
            {
                GameLogger.LogWarning("다음 적 데이터를 가져올 수 없습니다", GameLogger.LogCategory.Combat);
                return false;
            }

            isSpawning = true;
            
            try
            {
                // 적 생성 (단순화된 로직)
                var enemy = await CreateEnemyAsync(data);
                if (enemy == null)
                {
                    GameLogger.LogError("적 생성 실패", GameLogger.LogCategory.Combat);
                    return false;
                }

                RegisterEnemy(enemy);
                currentEnemyIndex++;
                
                GameLogger.LogInfo($"적 생성 완료: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError($"적 생성 중 오류 발생: {ex.Message}", GameLogger.LogCategory.Error);
                return false;
            }
            finally
            {
                isSpawning = false;
            }
        }

        /// <summary>
        /// 기존 API 호환성을 위한 동기 메서드
        /// </summary>
        public void SpawnNextEnemy()
        {
            _ = SpawnNextEnemyAsync();
        }

        /// <summary>
        /// 적 캐릭터를 시스템에 등록합니다.
        /// </summary>
        private void RegisterEnemy(ICharacter enemy)
        {
            var enemyManager = FindFirstObjectByType<EnemyManager>();
            enemyManager?.RegisterEnemy(enemy);
            
            // 적 캐릭터에 사망 리스너 설정
            if (enemy is EnemyCharacter concreteEnemy)
            {
                concreteEnemy.SetDeathListener(new EnemyDeathHandler(this));
            }
            
            GameLogger.LogInfo($"적 등록 완료: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 적 처치 시 호출되는 메서드
        /// </summary>
        public void OnEnemyDeath(ICharacter enemy)
        {
            GameLogger.LogInfo($"적 처치: {enemy.GetCharacterName()}", GameLogger.LogCategory.Combat);
            
            // 적 처치 이벤트 발생
            OnEnemyDefeated?.Invoke(enemy);
            
            // 스테이지 진행 상태 업데이트
            UpdateStageProgress(enemy);
        }

        /// <summary>
        /// 적 사망 처리를 위한 내부 클래스
        /// </summary>
        private class EnemyDeathHandler
        {
            private readonly StageManager stageManager;

            public EnemyDeathHandler(StageManager stageManager)
            {
                this.stageManager = stageManager;
            }

            public void OnCharacterDied(ICharacter character)
            {
                if (!character.IsPlayerControlled())
                {
                    stageManager.OnEnemyDeath(character);
                }
            }

            public void OnEnemyDeath(ICharacter enemy)
            {
                stageManager.OnEnemyDeath(enemy);
            }
        }


        /// <summary>
        /// 적 캐릭터 처치 후 스테이지 진행 상태를 업데이트합니다.
        /// 모든 적 처치 시 스테이지 완료(승리)를 처리합니다.
        /// </summary>
        private void UpdateStageProgress(ICharacter enemy)
        {
            // 다음 적이 있는지 확인
            if (HasMoreEnemies())
            {
                // 다음 적 생성
                _ = SpawnNextEnemyAsync();
            }
            else
            {
                // 모든 적 처치 완료 - 스테이지 승리!
                CompleteStage();
            }
        }

        /// <summary>
        /// 적 캐릭터를 생성합니다. (단순화된 로직)
        /// </summary>
        private async Task<ICharacter> CreateEnemyAsync(EnemyCharacterData data)
        {
            // 실제 적 생성 로직은 다른 시스템에 위임
            // 여기서는 단순히 데이터 검증만 수행
            if (data?.Prefab == null)
            {
                GameLogger.LogError("적 데이터 또는 프리팹이 null입니다", GameLogger.LogCategory.Error);
                return null;
            }

            // 비동기 처리 시뮬레이션
            await Task.Delay(100);
            
            // 실제 구현에서는 적 생성 로직을 호출
            // var enemy = Instantiate(data.Prefab).GetComponent<ICharacter>();
            // enemy.Initialize(data);
            // return enemy;
            
            // 임시로 null 반환 (실제 구현 시 수정 필요)
            return null;
        }

        /// <summary>
        /// 다음 적 데이터를 조회합니다.
        /// </summary>
        private bool TryGetNextEnemyData(out EnemyCharacterData data)
        {
            data = null;

            if (stageSettings.currentStage == null ||
                stageSettings.currentStage.enemies == null ||
                currentEnemyIndex >= stageSettings.currentStage.enemies.Count)
                return false;

            data = stageSettings.currentStage.enemies[currentEnemyIndex];
            return data != null && data.Prefab != null;
        }

        #endregion

        #region 스테이지 정보

        /// <inheritdoc />
        public StageData GetCurrentStage() => stageSettings.currentStage;

        /// <inheritdoc />
        public bool HasNextEnemy() =>
            stageSettings.currentStage != null && currentEnemyIndex < stageSettings.currentStage.enemies.Count;

        /// <summary>
        /// 아직 처치하지 않은 적이 더 있는지 확인합니다.
        /// </summary>
        private bool HasMoreEnemies()
        {
            return HasNextEnemy();
        }

        /// <inheritdoc />
        public EnemyCharacterData PeekNextEnemyData() =>
            HasNextEnemy() ? stageSettings.currentStage.enemies[currentEnemyIndex] : null;

        /// <summary>
        /// 현재 스테이지 번호를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        public void SetCurrentStageNumber(int stageNumber)
        {
            // TODO: 실제 스테이지 번호 관리 로직 구현 필요
            // 현재는 StageData의 name이나 다른 식별자를 사용할 수 있음
            GameLogger.LogInfo($"스테이지 번호 설정: {stageNumber}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 현재 스테이지 번호를 가져옵니다. (저장 시스템용)
        /// </summary>
        /// <returns>스테이지 번호</returns>
        public int GetCurrentStageNumber()
        {
            // TODO: 실제 스테이지 번호 반환 로직 구현 필요
            return 0; // 임시값
        }

        #endregion

        #region 로그 스쿨 시스템 - 단계별 관리

        #region 스테이지 진행 관리

        public StageProgressState ProgressState => progressState;
        public bool IsStageCompleted => isStageCompleted;

        /// <summary>
        /// 스테이지를 시작합니다. 첫 번째 적을 생성합니다.
        /// </summary>
        public void StartStage()
        {
            if (stageSettings.currentStage == null || stageSettings.currentStage.enemies.Count == 0)
            {
                GameLogger.LogWarning("스테이지 데이터가 유효하지 않습니다", GameLogger.LogCategory.Combat);
                return;
            }

            progressState = StageProgressState.InProgress;
            currentEnemyIndex = 0;
            isStageCompleted = false;
            
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogInfo($"스테이지 시작: {stageSettings.currentStage.name}", GameLogger.LogCategory.Combat);
            
            // 첫 번째 적 생성
            _ = SpawnNextEnemyAsync();
        }

        /// <summary>
        /// 스테이지를 완료합니다. 모든 적 처치 시 호출됩니다.
        /// </summary>
        public void CompleteStage()
        {
            progressState = StageProgressState.Completed;
            isStageCompleted = true;
            
            OnProgressChanged?.Invoke(progressState);
            
            // 스테이지 완료 이벤트 발생
            OnStageCompleted?.Invoke(stageSettings.currentStage);
            
            GameLogger.LogInfo($"스테이지 완료 (승리!): {stageSettings.currentStage.name}", GameLogger.LogCategory.Combat);
        }

        public void FailStage()
        {
            progressState = StageProgressState.Failed;
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogWarning($"스테이지 실패: {stageSettings.currentStage.name}", GameLogger.LogCategory.Combat);
        }

        public event System.Action<StageProgressState> OnProgressChanged;

        #endregion


        #endregion
    }
}

