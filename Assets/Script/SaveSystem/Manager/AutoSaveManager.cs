using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.SaveSystem.Data;
using Game.SaveSystem.Interface;
using Game.CoreSystem.Save;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// 자동 저장 매니저
    /// 슬레이 더 스파이어 방식: 특정 이벤트 발생 시 자동 저장
    /// </summary>
    public class AutoSaveManager : BaseSaveManager<IAutoSaveManager>
    {
        #region 의존성 주입

        [Inject] private ICardStateCollector cardStateCollector;
        [Inject] private ICardStateRestorer cardStateRestorer;
        [Inject] private SaveManager saveManager;

        #endregion

        #region AutoSaveManager 전용 설정

        [Header("자동 저장 설정")]
        [Tooltip("자동 저장 활성화")]
        [SerializeField] private bool autoSaveEnabled = true;
        [Tooltip("자동 저장 조건들")]
        [SerializeField] private List<AutoSaveCondition> autoSaveConditions = new();

        #endregion

        #region 내부 상태

        private Dictionary<string, AutoSaveCondition> conditionMap = new();
        private bool isInitialized = false;
        // 초기화 상태는 베이스 클래스에서 관리

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

            // 기본 자동 저장 조건들 추가
            if (autoSaveConditions.Count == 0)
            {
                AddDefaultAutoSaveConditions();
            }

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
            Debug.Log($"[AutoSaveManager] 자동 저장 조건 초기화 완료: {conditionMap.Count}개");
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
        /// 수동으로 게임 상태를 저장합니다.
        /// </summary>
        /// <param name="saveName">저장 이름</param>
        public async void SaveGameState(string saveName = "ManualSave")
        {
            try
            {
                Debug.Log($"[AutoSaveManager] 수동 저장 시작: {saveName}");

                // 카드 상태 수집
                var cardState = cardStateCollector.CollectCompleteCardState(saveName);
                
                if (!cardState.IsValid())
                {
                    Debug.LogError("[AutoSaveManager] 수집된 카드 상태가 유효하지 않습니다.");
                    return;
                }

                // JSON으로 직렬화
                string jsonData = JsonUtility.ToJson(cardState, true);
                
                // 파일로 저장
                string fileName = $"ManualSave_{saveName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
                
                await System.IO.File.WriteAllTextAsync(filePath, jsonData);
                
                Debug.Log($"[AutoSaveManager] 수동 저장 완료: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AutoSaveManager] 수동 저장 실패: {ex.Message}");
            }
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
