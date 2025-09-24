using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Manager;
using Game.CombatSystem.Interface;
using Game.StageSystem.Data;
using Game.StageSystem.Interface;
using Zenject;
using Game.CoreSystem.Utility;
using DG.Tweening;
using Game.CoreSystem.Audio;

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
            [Tooltip("모든 스테이지 데이터 (1-4번 스테이지)")]
            public List<StageData> allStages = new List<StageData>();

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
        
        // 다중 스테이지 관리
        private StageData currentStage;
        private int totalStagesCompleted = 0;
        private bool isGameCompleted = false;

        #endregion

        #region 이벤트

        /// <summary>적 처치 시 호출되는 이벤트</summary>
        public event Action<ICharacter> OnEnemyDefeated;
        
        /// <summary>스테이지 완료 시 호출되는 이벤트</summary>
        public event Action<StageData> OnStageCompleted;
        
        /// <summary>게임 완료 시 호출되는 이벤트 (모든 스테이지 완료)</summary>
        public event Action OnGameCompleted;
        
        /// <summary>스테이지 전환 시 호출되는 이벤트</summary>
        public event Action<StageData, StageData> OnStageTransition;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 스테이지 매니저 초기화
        /// GameStartupController에서 수동으로 시작하므로 자동 시작 제거
        /// </summary>
        private void Start()
        {
            // GameStartupController에서 수동으로 시작하므로 자동 시작 제거
            // 기본 스테이지만 로드하고 시작은 GameStartupController에서 처리
            if (LoadStage(1))
            {
                GameLogger.LogInfo("기본 스테이지 로드 완료 - GameStartupController에서 시작 대기", GameLogger.LogCategory.Combat);
            }
            else
            {
                GameLogger.LogError("기본 스테이지 로드 실패", GameLogger.LogCategory.Combat);
            }
        }

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

                // 적 전용 BGM이 설정되어 있으면 전환
                if (data.EnemyBGM != null)
                {
                    var audioManager = FindFirstObjectByType<AudioManager>();
                    if (audioManager != null)
                    {
                        audioManager.PlayBGM(data.EnemyBGM, true);
                    }
                }
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
        /// 적 캐릭터를 생성합니다.
        /// </summary>
        private async Task<ICharacter> CreateEnemyAsync(EnemyCharacterData data)
        {
            if (data?.Prefab == null)
            {
                GameLogger.LogError("적 데이터 또는 프리팹이 null입니다", GameLogger.LogCategory.Error);
                return null;
            }

            // 비동기 처리 시뮬레이션
            await Task.Delay(100);
            
            // EnemyManager의 characterSlot을 찾아서 적을 배치
            var enemyManager = FindFirstObjectByType<EnemyManager>();
            if (enemyManager == null)
            {
                GameLogger.LogError("EnemyManager를 찾을 수 없습니다", GameLogger.LogCategory.Error);
                return null;
            }

            var characterSlot = enemyManager.GetCharacterSlot();
            if (characterSlot == null)
            {
                GameLogger.LogError("EnemyManager의 characterSlot을 찾을 수 없습니다", GameLogger.LogCategory.Error);
                return null;
            }

            // 적 프리팹 인스턴스 생성 (characterSlot에 배치)
            var enemyInstance = Instantiate(data.Prefab, characterSlot);
            enemyInstance.name = data.name; // ScriptableObject의 이름 사용
            
            // ICharacter 컴포넌트 확인
            if (!enemyInstance.TryGetComponent(out ICharacter enemy))
            {
                GameLogger.LogError($"적 프리팹에 ICharacter 컴포넌트가 없습니다: {data.CharacterName}", GameLogger.LogCategory.Error);
                Destroy(enemyInstance);
                return null;
            }
            
            // 적 데이터 설정
            enemy.SetCharacterData(data);
            // 등장 연출 (오른쪽 바깥에서 자리로) - Ease.InOutCubic 그래프
            TryPlayEntranceAnimation(enemyInstance.transform, fromLeft: false);
            
            GameLogger.LogInfo($"적 캐릭터 생성 및 배치 완료: {data.CharacterName} (슬롯: {characterSlot.name})", GameLogger.LogCategory.Combat);
            return enemy;
        }

        /// <summary>
        /// 캐릭터가 화면 밖에서 슬라이드 인 되는 연출을 수행합니다.
        /// RectTransform이 있으면 DOAnchorPos, 아니면 DOMove를 사용합니다.
        /// </summary>
        private Tween TryPlayEntranceAnimation(Transform target, bool fromLeft)
        {
            if (target == null) return null;
            const float duration = 1.5f;
            var ease = Ease.InOutCubic;

            if (target is RectTransform rt)
            {
                Vector2 end = rt.anchoredPosition;
                Vector2 start = new Vector2(fromLeft ? -1100f : 1100f, end.y);
                rt.anchoredPosition = start;
                return rt.DOAnchorPos(end, duration).SetEase(ease);
            }
            else
            {
                Vector3 end = target.position;
                Vector3 start = new Vector3(fromLeft ? -1100f : 1100f, end.y, end.z);
                target.position = start;
                return target.DOMove(end, duration).SetEase(ease);
            }
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

        /// <summary>
        /// 아직 처치하지 않은 적이 더 있는지 확인합니다.
        /// </summary>
        private bool HasMoreEnemies()
        {
            return HasNextEnemy();
        }

        /// <inheritdoc />
        public EnemyCharacterData PeekNextEnemyData() =>
            HasNextEnemy() ? currentStage.enemies[currentEnemyIndex] : null;

        /// <summary>
        /// 현재 스테이지 번호를 설정합니다. (저장 시스템용)
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        public void SetCurrentStageNumber(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > 4)
            {
                GameLogger.LogError($"잘못된 스테이지 번호: {stageNumber}", GameLogger.LogCategory.Combat);
                return;
            }
            
            LoadStage(stageNumber);
            GameLogger.LogInfo($"스테이지 번호 설정: {stageNumber}", GameLogger.LogCategory.Combat);
        }

        /// <summary>
        /// 현재 스테이지 번호를 가져옵니다. (저장 시스템용)
        /// </summary>
        /// <returns>스테이지 번호</returns>
        public int GetCurrentStageNumber()
        {
            return currentStage?.stageNumber ?? 1;
        }
        
        /// <summary>
        /// 다음 스테이지가 있는지 확인합니다.
        /// </summary>
        public bool HasNextStage()
        {
            return currentStage?.stageNumber < 4;
        }
        
        /// <summary>
        /// 다음 스테이지로 진행합니다.
        /// </summary>
        public bool ProgressToNextStage()
        {
            if (!HasNextStage())
            {
                GameLogger.LogWarning("더 이상 진행할 스테이지가 없습니다", GameLogger.LogCategory.Combat);
                return false;
            }
            
            int nextStageNumber = (currentStage?.stageNumber ?? 1) + 1;
            return LoadStage(nextStageNumber);
        }
        
        /// <summary>
        /// 특정 스테이지를 로드합니다.
        /// </summary>
        /// <param name="stageNumber">로드할 스테이지 번호</param>
        public bool LoadStage(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > 4)
            {
                GameLogger.LogError($"잘못된 스테이지 번호: {stageNumber}", GameLogger.LogCategory.Combat);
                return false;
            }
            
            var stageData = GetStageData(stageNumber);
            if (stageData == null)
            {
                GameLogger.LogError($"스테이지 {stageNumber} 데이터를 찾을 수 없습니다", GameLogger.LogCategory.Combat);
                return false;
            }
            
            if (!stageData.IsValid())
            {
                GameLogger.LogError($"스테이지 {stageNumber} 데이터가 유효하지 않습니다", GameLogger.LogCategory.Combat);
                return false;
            }
            
            // 이전 스테이지 저장
            var previousStage = currentStage;
            
            // 새 스테이지 설정
            currentStage = stageData;
            currentEnemyIndex = 0;
            isStageCompleted = false;
            progressState = StageProgressState.NotStarted;
            
            // 스테이지 전환 이벤트 발생
            if (previousStage != null)
            {
                OnStageTransition?.Invoke(previousStage, currentStage);
            }
            
            GameLogger.LogInfo($"스테이지 {stageNumber} 로드 완료: {currentStage.stageName}", GameLogger.LogCategory.Combat);
            return true;
        }
        
        /// <summary>
        /// 특정 번호의 스테이지 데이터를 가져옵니다.
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        /// <returns>스테이지 데이터</returns>
        private StageData GetStageData(int stageNumber)
        {
            if (stageSettings.allStages == null || stageSettings.allStages.Count == 0)
            {
                GameLogger.LogError("스테이지 데이터가 설정되지 않았습니다", GameLogger.LogCategory.Combat);
                return null;
            }
            
            foreach (var stage in stageSettings.allStages)
            {
                if (stage != null && stage.stageNumber == stageNumber)
                {
                    return stage;
                }
            }
            
            return null;
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
            if (currentStage == null || currentStage.enemies.Count == 0)
            {
                GameLogger.LogWarning("스테이지 데이터가 유효하지 않습니다", GameLogger.LogCategory.Combat);
                return;
            }

            progressState = StageProgressState.InProgress;
            currentEnemyIndex = 0;
            isStageCompleted = false;
            
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogInfo($"스테이지 시작: {currentStage.stageName} (스테이지 {currentStage.stageNumber})", GameLogger.LogCategory.Combat);
            
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
            totalStagesCompleted++;
            
            OnProgressChanged?.Invoke(progressState);
            
            // 스테이지 완료 이벤트 발생
            OnStageCompleted?.Invoke(currentStage);
            
            GameLogger.LogInfo($"스테이지 완료 (승리!): {currentStage.stageName} (스테이지 {currentStage.stageNumber})", GameLogger.LogCategory.Combat);
            
            // 다음 스테이지로 진행 또는 게임 완료 처리
            if (currentStage.IsLastStage)
            {
                // 마지막 스테이지 완료 - 게임 완료!
                CompleteGame();
            }
            else if (currentStage.autoProgressToNext)
            {
                // 다음 스테이지로 자동 진행 (즉시)
                if (ProgressToNextStage())
                {
                    GameLogger.LogInfo($"다음 스테이지로 진행: {currentStage.stageName}", GameLogger.LogCategory.Combat);
                    StartStage();
                }
            }
        }
        
        /// <summary>
        /// 게임을 완료합니다. (모든 스테이지 완료)
        /// </summary>
        private void CompleteGame()
        {
            isGameCompleted = true;
            OnGameCompleted?.Invoke();
            GameLogger.LogInfo("🎉 게임 완료! 모든 스테이지를 클리어했습니다!", GameLogger.LogCategory.Combat);
        }
        

        public void FailStage()
        {
            progressState = StageProgressState.Failed;
            OnProgressChanged?.Invoke(progressState);
            
            GameLogger.LogWarning($"스테이지 실패: {currentStage?.stageName ?? "Unknown"} (스테이지 {currentStage?.stageNumber ?? 1})", GameLogger.LogCategory.Combat);
        }

        public event System.Action<StageProgressState> OnProgressChanged;

        #endregion

        #region 게임 상태 정보

        /// <summary>
        /// 게임이 완료되었는지 확인합니다.
        /// </summary>
        public bool IsGameCompleted => isGameCompleted;

        /// <summary>
        /// 완료된 스테이지 수를 가져옵니다.
        /// </summary>
        public int TotalStagesCompleted => totalStagesCompleted;

        /// <summary>
        /// 전체 스테이지 수를 가져옵니다.
        /// </summary>
        public int TotalStages => 4;

        /// <summary>
        /// 게임 진행률을 가져옵니다. (0.0 ~ 1.0)
        /// </summary>
        public float GameProgress => (float)totalStagesCompleted / TotalStages;

        /// <summary>
        /// 특정 스테이지 데이터를 가져옵니다. (public 버전)
        /// </summary>
        /// <param name="stageNumber">스테이지 번호</param>
        /// <returns>스테이지 데이터</returns>
        public StageData GetStageDataPublic(int stageNumber)
        {
            return GetStageData(stageNumber);
        }

        /// <summary>
        /// 모든 스테이지 데이터를 가져옵니다.
        /// </summary>
        /// <returns>모든 스테이지 데이터</returns>
        public List<StageData> GetAllStages()
        {
            return stageSettings.allStages ?? new List<StageData>();
        }

        #endregion

        #endregion
    }
}

