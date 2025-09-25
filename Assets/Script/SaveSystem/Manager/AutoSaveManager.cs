using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.SaveSystem.Data;
using Game.SaveSystem.Interface;
using Game.CoreSystem.Save;
using Game.CoreSystem.Utility;
using Zenject;
using System.IO;
using System.Threading.Tasks;
using Game.CombatSystem;
using Game.CombatSystem.Manager;
using Game.StageSystem.Manager;
using UnityEngine.SceneManagement;

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// 자동 저장 매니저
    /// 슬레이 더 스파이어 방식: 특정 이벤트 발생 시 자동 저장
    /// </summary>
    public class AutoSaveManager : BaseSaveManager<IAutoSaveManager>
    {
        #region 의존성 주입

        [Inject(Optional = true)] private ICardStateCollector cardStateCollector;
        [Inject(Optional = true)] private ICardStateRestorer cardStateRestorer;
        [Inject] private SaveManager saveManager;

        #endregion

        #region AutoSaveManager 전용 설정

        [Header("자동 저장 설정")]
        [Tooltip("자동 저장 활성화(이벤트 기반)")]
        [SerializeField] private bool autoSaveEnabled = true;
        [Tooltip("턴/스테이지 경계에서만 자동 저장(시간 기반 없음)")]
        [SerializeField] private bool turnBasedAutosaveEnabled = true;
        [Tooltip("자동 저장 조건들")]
        [SerializeField] private List<AutoSaveCondition> autoSaveConditions = new();

        #endregion

        #region 내부 상태

        private Dictionary<string, AutoSaveCondition> conditionMap = new();
        private bool isInitialized = false;
        // 초기화 상태는 베이스 클래스에서 관리
        private TurnManager turnManager;
        private StageManager stageManager;
        private int lastSavedFrame = -1;
        private string lastSavedTrigger = "";

        #endregion

        #region 초기화

        protected override void Start()
        {
            base.Start();
            // 베이스 클래스에서 자동 초기화 처리
        }

        #region 베이스 클래스 구현

        protected override System.Collections.IEnumerator OnInitialize()
        {
            // 자동 저장 조건 초기화
            InitializeAutoSaveConditions();

            // 참조 검증
            ValidateReferences();

            // 저장 UI 연결
            ConnectSaveUI();

            // 매니저 상태 로깅
            LogManagerState();

            // 카드게임 특성상 시간 기반 저장은 사용하지 않음. 턴/스테이지 이벤트 기반만 선택적으로 활성화
            if (turnBasedAutosaveEnabled)
            {
                HookRuntimeDependencies();
                SceneManager.sceneLoaded += OnSceneLoaded;
                GameLogger.LogInfo("[AutoSaveManager] 턴/스테이지 이벤트 기반 자동 저장 활성화(씬 전환 대응)", GameLogger.LogCategory.Save);
            }
            else
            {
                GameLogger.LogInfo("[AutoSaveManager] 자동 저장 비활성화 상태로 시작합니다.", GameLogger.LogCategory.Save);
            }

            yield return null;
        }

        public override void Reset()
        {
            // 조건 맵 정리
            conditionMap.Clear();

            // 자동 저장 조건 재초기화
            InitializeAutoSaveConditions();

            if (enableDebugLogging)
            {
                GameLogger.LogInfo("AutoSaveManager 리셋 완료", GameLogger.LogCategory.Save);
            }
        }

        private async void OnTurnChangedHandler(TurnManager.TurnType _)
        {
            if (!autoSaveEnabled) return;
            if (!ShouldSave("TurnChanged")) return;
            GameLogger.LogInfo("[AutoSaveManager] 턴 변경 트리거 감지 → 저장 시도(TurnChanged)", GameLogger.LogCategory.Save);
            await TrySaveWithRetry("TurnChanged");
        }

        private async void OnStageCompletedHandler(Game.StageSystem.Data.StageData _)
        {
            if (!autoSaveEnabled) return;
            if (!ShouldSave("StageCompleted")) return;
            GameLogger.LogInfo("[AutoSaveManager] 스테이지 완료 트리거 감지 → 저장 시도(StageCompleted)", GameLogger.LogCategory.Save);
            await SaveGameStateAsync("StageCompleted");
        }

        private async void OnTurnStarted()
        {
            if (!autoSaveEnabled) return;
            if (!ShouldSave("TurnStart")) return;
            GameLogger.LogInfo("[AutoSaveManager] 턴 시작 트리거 감지 → 저장 시도(TurnStart)", GameLogger.LogCategory.Save);
            await SaveGameStateAsync("TurnStart");
        }

        private async void OnTurnEnded()
        {
            if (!autoSaveEnabled) return;
            if (!ShouldSave("TurnEnd")) return;
            GameLogger.LogInfo("[AutoSaveManager] 턴 종료 트리거 감지 → 저장 시도(TurnEnd)", GameLogger.LogCategory.Save);
            await TrySaveWithRetry("TurnEnd");
        }

        private async Task TrySaveWithRetry(string trigger)
        {
            // 최대 30프레임(약 0.5초@60fps)까지 준비 완료 대기
            for (int i = 0; i < 60; i++)
            {
                if (cardStateCollector is CardStateCollector c && c.IsRuntimeReady())
                {
                    await SaveGameStateAsync(trigger);
                    return;
                }
                else if (cardStateCollector is CardStateCollector c2)
                {
                    // 첫 1회 이유 로깅
                    if (i == 0)
                    {
                        c2.LogNotReadyReasons();
                    }
                }
                await Task.Yield();
            }
            GameLogger.LogWarning("[AutoSaveManager] 저장 보류: 런타임 준비 미완료(타임아웃)", GameLogger.LogCategory.Save);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!turnBasedAutosaveEnabled) return;
            HookRuntimeDependencies();
            GameLogger.LogInfo($"[AutoSaveManager] 씬 로드 감지 → 저장 이벤트 재구독: {scene.name}", GameLogger.LogCategory.Save);
        }

        private void HookRuntimeDependencies()
        {
            // 이전 구독 해제
            if (turnManager != null)
            {
                turnManager.OnTurnChanged -= OnTurnChangedHandler;
            }
            if (stageManager != null)
            {
                stageManager.OnStageCompleted -= OnStageCompletedHandler;
            }

            // 재탐색 및 구독
            turnManager = UnityEngine.Object.FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                turnManager.OnTurnChanged += OnTurnChangedHandler;
            }

            stageManager = UnityEngine.Object.FindFirstObjectByType<StageManager>();
            if (stageManager != null)
            {
                stageManager.OnStageCompleted += OnStageCompletedHandler;
            }
        }

        private bool ShouldSave(string trigger)
        {
            int frame = Time.frameCount;
            if (frame == lastSavedFrame && trigger == lastSavedTrigger)
            {
                return false;
            }
            lastSavedFrame = frame;
            lastSavedTrigger = trigger;
            return true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (turnManager != null)
            {
                turnManager.OnTurnChanged -= OnTurnChangedHandler;
            }
            if (stageManager != null)
            {
                stageManager.OnStageCompleted -= OnStageCompletedHandler;
            }
        }

        #endregion

        /// <summary>
        /// 자동 저장 조건들을 초기화합니다.
        /// </summary>
        private void InitializeAutoSaveConditions()
        {
            if (autoSaveConditions == null)
            {
                autoSaveConditions = new List<AutoSaveCondition>();
            }

            // 시간/주기 기반 자동 저장 조건은 추가하지 않습니다(이벤트 기반만 사용).

            // 조건 맵 생성
            conditionMap.Clear();
            foreach (var condition in autoSaveConditions)
            {
                if (condition.IsValid())
                {
                    conditionMap[condition.conditionName] = condition;
                }
            }

            isInitialized = true;
            Debug.Log($"[AutoSaveManager] 자동 저장 조건 초기화 완료(비활성화): {conditionMap.Count}개");
        }

        /// <summary>
        /// 기본 자동 저장 조건들을 추가합니다.
        /// </summary>
        private void AddDefaultAutoSaveConditions()
        {
            autoSaveConditions.Add(new AutoSaveCondition(
                "EnemyCardPlaced",
                Game.SaveSystem.Data.AutoSaveTrigger.EnemyCardPlaced,
                "적이 카드를 올려놓은 후 자동 저장"
            ));

            autoSaveConditions.Add(new AutoSaveCondition(
                "BeforeTurnStart",
                Game.SaveSystem.Data.AutoSaveTrigger.BeforeTurnStart,
                "턴 시작 버튼 누르기 전 자동 저장"
            ));

            autoSaveConditions.Add(new AutoSaveCondition(
                "DuringTurnExecution",
                Game.SaveSystem.Data.AutoSaveTrigger.DuringTurnExecution,
                "턴 실행 중 자동 저장"
            ));

            autoSaveConditions.Add(new AutoSaveCondition(
                "TurnCompleted",
                Game.SaveSystem.Data.AutoSaveTrigger.TurnCompleted,
                "턴 완료 후 자동 저장"
            ));

            autoSaveConditions.Add(new AutoSaveCondition(
                "StageCompleted",
                Game.SaveSystem.Data.AutoSaveTrigger.StageCompleted,
                "스테이지 완료 후 자동 저장"
            ));

            autoSaveConditions.Add(new AutoSaveCondition(
                "GameExit",
                Game.SaveSystem.Data.AutoSaveTrigger.GameExit,
                "게임 종료 시 자동 저장"
            ));
        }

        #endregion

        #region 자동 저장 트리거

        /// <summary>
        /// 자동 저장을 트리거합니다.
        /// </summary>
        /// <param name="triggerName">트리거 이름</param>
        public void TriggerAutoSave(string triggerName)
        {
            if (!autoSaveEnabled || !isInitialized)
            {
                Debug.LogWarning("[AutoSaveManager] 자동 저장이 비활성화되어 있습니다.");
                return;
            }

            if (!conditionMap.TryGetValue(triggerName, out var condition))
            {
                Debug.LogWarning($"[AutoSaveManager] 알 수 없는 자동 저장 트리거: {triggerName}");
                return;
            }

            if (!condition.isEnabled)
            {
                Debug.Log($"[AutoSaveManager] 자동 저장 조건이 비활성화됨: {triggerName}");
                return;
            }

            PerformAutoSave(condition);
        }

        /// <summary>
        /// 자동 저장을 수행합니다.
        /// </summary>
        /// <param name="condition">자동 저장 조건</param>
        private async void PerformAutoSave(AutoSaveCondition condition)
        {
            try
            {
                Debug.Log($"[AutoSaveManager] 자동 저장 시작: {condition.conditionName}");

                // 카드 상태 수집
                var cardState = cardStateCollector.CollectCompleteCardState(condition.conditionName);
                
                if (!cardState.IsValid())
                {
                    Debug.LogError("[AutoSaveManager] 수집된 카드 상태가 유효하지 않습니다.");
                    return;
                }

                // JSON으로 직렬화
                string jsonData = JsonUtility.ToJson(cardState, true);
                
                // 파일로 저장
                string fileName = $"AutoSave_{condition.conditionName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
                
                await System.IO.File.WriteAllTextAsync(filePath, jsonData);
                
                Debug.Log($"[AutoSaveManager] 자동 저장 완료: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AutoSaveManager] 자동 저장 실패: {ex.Message}");
            }
        }

        #endregion

        #region 수동 저장/로드

        /// <summary>
        /// 수동으로 게임 상태를 저장합니다. (await 가능)
        /// </summary>
        /// <param name="saveName">저장 이름</param>
        public async Task<bool> SaveGameStateAsync(string saveName = "ManualSave")
        {
            try
            {
                GameLogger.LogInfo($"[AutoSaveManager] 수동 저장 시작: {saveName}", GameLogger.LogCategory.Save);

                var cardState = cardStateCollector.CollectCompleteCardState(saveName);
                if (cardState == null || !cardState.IsValid())
                {
                    GameLogger.LogError("[AutoSaveManager] 수집된 카드 상태가 유효하지 않습니다.", GameLogger.LogCategory.Save);
                    return false;
                }

                string jsonData = JsonUtility.ToJson(cardState, true);
                string fileName = $"ManualSave_{saveName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);

                await System.IO.File.WriteAllTextAsync(filePath, jsonData);
                GameLogger.LogInfo($"[AutoSaveManager] 수동 저장 완료: {filePath}", GameLogger.LogCategory.Save);
                return true;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[AutoSaveManager] 수동 저장 실패: {ex.Message}", GameLogger.LogCategory.Save);
                return false;
            }
        }

        /// <summary>
        /// 기존 시그니처(호환성): fire-and-forget 저장. 가능하면 SaveGameStateAsync를 사용하세요.
        /// </summary>
        public async void SaveGameState(string saveName = "ManualSave")
        {
            await SaveGameStateAsync(saveName);
        }

        /// <summary>
        /// 저장된 게임 상태를 로드합니다.
        /// </summary>
        /// <param name="filePath">저장 파일 경로</param>
        public async void LoadGameState(string filePath)
        {
            try
            {
                Debug.Log($"[AutoSaveManager] 게임 상태 로드 시작: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    Debug.LogError($"[AutoSaveManager] 저장 파일이 존재하지 않습니다: {filePath}");
                    return;
                }

                // 파일에서 데이터 읽기
                string jsonData = await System.IO.File.ReadAllTextAsync(filePath);
                
                // JSON 역직렬화
                var cardState = JsonUtility.FromJson<CompleteCardStateData>(jsonData);
                
                if (!cardState.IsValid())
                {
                    Debug.LogError("[AutoSaveManager] 로드된 카드 상태가 유효하지 않습니다.");
                    return;
                }

                // 카드 상태 복원
                bool success = cardStateRestorer.RestoreCompleteCardState(cardState);
                
                if (success)
                {
                    Debug.Log($"[AutoSaveManager] 게임 상태 로드 완료: {filePath}");
                }
                else
                {
                    Debug.LogError("[AutoSaveManager] 게임 상태 로드 실패");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AutoSaveManager] 게임 상태 로드 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 가장 최근 자동/수동 저장 파일을 찾아 로드 및 복원합니다.
        /// 셋업 과정을 우회하기 위해 전투 씬 진입 직후 호출하십시오.
        /// </summary>
        public async Task<bool> LoadLatestAutoSaveAsync()
        {
            try
            {
                string dir = Application.persistentDataPath;
                if (!Directory.Exists(dir))
                {
                    GameLogger.LogWarning("[AutoSaveManager] 저장 디렉토리가 없습니다.", GameLogger.LogCategory.Save);
                    return false;
                }

                // AutoSave_*.json, ManualSave_*.json 중 최신 파일 선택
                var files = Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly)
                    .Where(p => Path.GetFileName(p).StartsWith("AutoSave_") || Path.GetFileName(p).StartsWith("ManualSave_"))
                    .ToList();

                if (files.Count == 0)
                {
                    GameLogger.LogWarning("[AutoSaveManager] 저장 파일이 없습니다.", GameLogger.LogCategory.Save);
                    return false;
                }

                string latest = files
                    .OrderByDescending(p => File.GetLastWriteTimeUtc(p))
                    .First();

                string jsonData = await File.ReadAllTextAsync(latest);
                var cardState = JsonUtility.FromJson<CompleteCardStateData>(jsonData);
                if (cardState == null || !cardState.IsValid())
                {
                    GameLogger.LogError("[AutoSaveManager] 최신 저장 데이터가 유효하지 않습니다.", GameLogger.LogCategory.Save);
                    return false;
                }

                bool success = cardStateRestorer.RestoreCompleteCardState(cardState);

                // 초기 셋업 우회: 전투 턴 매니저 셋업 완료 플래그 강제 설정
                var turn = UnityEngine.Object.FindFirstObjectByType<Game.CombatSystem.Manager.TurnManager>();
                if (turn != null)
                {
                    turn._initialSlotSetupCompleted = true;
                }

                if (success)
                {
                    GameLogger.LogInfo($"[AutoSaveManager] 최신 저장 로드 완료: {Path.GetFileName(latest)}", GameLogger.LogCategory.Save);
                }
                else
                {
                    GameLogger.LogError("[AutoSaveManager] 최신 저장 로드 실패(복원 실패)", GameLogger.LogCategory.Save);
                }

                return success;
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[AutoSaveManager] 최신 저장 로드 실패: {ex.Message}", GameLogger.LogCategory.Save);
                return false;
            }
        }

        #endregion

        #region 설정 관리

        /// <summary>
        /// 자동 저장 활성화/비활성화
        /// </summary>
        /// <param name="enabled">활성화 여부</param>
        public void SetAutoSaveEnabled(bool enabled)
        {
            autoSaveEnabled = enabled;
            Debug.Log($"[AutoSaveManager] 자동 저장 {(enabled ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// 자동 저장이 활성화되어 있는지 확인
        /// </summary>
        public bool IsAutoSaveEnabled()
        {
            return autoSaveEnabled;
        }

        /// <summary>
        /// 특정 자동 저장 조건 활성화/비활성화
        /// </summary>
        /// <param name="conditionName">조건 이름</param>
        /// <param name="enabled">활성화 여부</param>
        public void SetConditionEnabled(string conditionName, bool enabled)
        {
            if (conditionMap.TryGetValue(conditionName, out var condition))
            {
                condition.isEnabled = enabled;
                Debug.Log($"[AutoSaveManager] 자동 저장 조건 {(enabled ? "활성화" : "비활성화")}: {conditionName}");
            }
            else
            {
                Debug.LogWarning($"[AutoSaveManager] 알 수 없는 자동 저장 조건: {conditionName}");
            }
        }

        #endregion

        #region 디버그

        /// <summary>
        /// 현재 자동 저장 설정을 출력합니다.
        /// </summary>
        [ContextMenu("자동 저장 설정 출력")]
        public void PrintAutoSaveSettings()
        {
            Debug.Log($"[AutoSaveManager] 자동 저장 활성화: {autoSaveEnabled}");
            Debug.Log($"[AutoSaveManager] 등록된 조건 수: {conditionMap.Count}");
            
            foreach (var kvp in conditionMap)
            {
                var condition = kvp.Value;
                Debug.Log($"  - {condition.conditionName}: {(condition.isEnabled ? "활성화" : "비활성화")} ({condition.trigger})");
            }
        }

        #endregion
    }
}
