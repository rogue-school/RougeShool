using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.SaveSystem.Data;
using Game.CoreSystem.Save;
using Game.CoreSystem.Utility;
using Zenject;
using System.Threading.Tasks;
using Game.CombatSystem.Manager;
using Game.StageSystem.Manager;
using Game.CharacterSystem.Interface;
using UnityEngine.SceneManagement;

namespace Game.SaveSystem.Manager
{
    /// <summary>
    /// 자동 저장 매니저 (간소화 버전)
    /// 이벤트 기반 자동 저장: 턴 종료, 스테이지 전환 시 자동 저장
    /// </summary>
    public class AutoSaveManager : MonoBehaviour
    {
        #region 의존성 주입

        [Inject] private SaveManager saveManager;
        [Inject(Optional = true)] private TurnManager turnManager;
        [Inject(Optional = true)] private StageManager stageManager;

        #endregion

        #region AutoSaveManager 전용 설정

        [Header("자동 저장 설정")]
        [Tooltip("자동 저장 활성화(이벤트 기반)")]
        [SerializeField] private bool autoSaveEnabled = true;
        [Tooltip("턴/스테이지 경계에서만 자동 저장(시간 기반 없음)")]
        [SerializeField] private bool turnBasedAutosaveEnabled = true;

        #endregion

        #region 내부 상태

        private bool isInitialized = false;
        private int lastSavedFrame = -1;
        private string lastSavedTrigger = "";

        #endregion

        #region 초기화

        private void Start()
        {
            StartCoroutine(Initialize());
        }

        private System.Collections.IEnumerator Initialize()
        {
            GameLogger.LogInfo("[AutoSaveManager] 초기화 시작", GameLogger.LogCategory.Save);

            // 이벤트 연결
            if (turnBasedAutosaveEnabled && autoSaveEnabled)
            {
                HookRuntimeDependencies();
                SceneManager.sceneLoaded += OnSceneLoaded;
                GameLogger.LogInfo("[AutoSaveManager] 턴/스테이지 이벤트 기반 자동 저장 활성화", GameLogger.LogCategory.Save);
            }
            else
            {
                GameLogger.LogInfo("[AutoSaveManager] 자동 저장 비활성화 상태로 시작합니다.", GameLogger.LogCategory.Save);
            }

            isInitialized = true;
            GameLogger.LogInfo("[AutoSaveManager] 초기화 완료", GameLogger.LogCategory.Save);
            yield return null;
        }

        private void OnDestroy()
        {
            // 이벤트 해제
            UnhookRuntimeDependencies();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #endregion

        #region 이벤트 연결

        private void HookRuntimeDependencies()
        {
            // 턴 매니저 이벤트 연결
            if (turnManager != null)
            {
                turnManager.OnTurnChanged += OnTurnChanged;
                GameLogger.LogInfo("[AutoSaveManager] 턴 매니저 이벤트 연결 완료", GameLogger.LogCategory.Save);
            }

            // 스테이지 매니저 이벤트 연결
            if (stageManager != null)
            {
                stageManager.OnStageCompleted += OnStageCompleted;
                stageManager.OnEnemyDefeated += OnEnemyDefeated;
                GameLogger.LogInfo("[AutoSaveManager] 스테이지 매니저 이벤트 연결 완료", GameLogger.LogCategory.Save);
            }
        }

        private void UnhookRuntimeDependencies()
        {
            // 턴 매니저 이벤트 해제
            if (turnManager != null)
            {
                turnManager.OnTurnChanged -= OnTurnChanged;
            }

            // 스테이지 매니저 이벤트 해제
            if (stageManager != null)
            {
                stageManager.OnStageCompleted -= OnStageCompleted;
                stageManager.OnEnemyDefeated -= OnEnemyDefeated;
            }
        }

        #endregion

        #region 자동 저장 트리거

        private async void OnTurnChanged(TurnManager.TurnType turnType)
        {
            if (!autoSaveEnabled || !isInitialized) return;

            // 중복 저장 방지
            if (Time.frameCount == lastSavedFrame) return;

            try
            {
                lastSavedFrame = Time.frameCount;
                lastSavedTrigger = "TurnChanged";

                await saveManager.SaveCurrentProgress("TurnChanged");
                GameLogger.LogInfo("[AutoSaveManager] 턴 변경 자동 저장 완료", GameLogger.LogCategory.Save);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[AutoSaveManager] 턴 변경 자동 저장 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
        }

        private async void OnStageCompleted(Game.StageSystem.Data.StageData stageData)
        {
            if (!autoSaveEnabled || !isInitialized) return;

            try
            {
                lastSavedTrigger = "StageCompleted";

                await saveManager.SaveCurrentProgress("StageCompleted");
                GameLogger.LogInfo("[AutoSaveManager] 스테이지 완료 자동 저장 완료", GameLogger.LogCategory.Save);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[AutoSaveManager] 스테이지 완료 자동 저장 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
        }

        private async void OnEnemyDefeated(Game.CharacterSystem.Interface.ICharacter enemy)
        {
            if (!autoSaveEnabled || !isInitialized) return;

            try
            {
                lastSavedTrigger = "EnemyDefeated";

                await saveManager.SaveCurrentProgress("EnemyDefeated");
                GameLogger.LogInfo("[AutoSaveManager] 적 처치 자동 저장 완료", GameLogger.LogCategory.Save);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[AutoSaveManager] 적 처치 자동 저장 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 씬 로드 시 매니저 재찾기
            if (scene.name == "StageScene")
            {
                StartCoroutine(ReinitializeForScene());
            }
        }

        private System.Collections.IEnumerator ReinitializeForScene()
        {
            yield return new WaitForSeconds(0.1f);

            HookRuntimeDependencies();

            GameLogger.LogInfo("[AutoSaveManager] 스테이지 씬용 재초기화 완료", GameLogger.LogCategory.Save);
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 수동 자동 저장 실행
        /// </summary>
        public async Task TriggerManualAutoSave()
        {
            if (!autoSaveEnabled || !isInitialized) return;

            try
            {
                await saveManager.SaveCurrentProgress("Manual");
                GameLogger.LogInfo("[AutoSaveManager] 수동 자동 저장 완료", GameLogger.LogCategory.Save);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"[AutoSaveManager] 수동 자동 저장 실패: {ex.Message}", GameLogger.LogCategory.Save);
            }
        }

        /// <summary>
        /// 자동 저장 활성화/비활성화
        /// </summary>
        public void SetAutoSaveEnabled(bool enabled)
        {
            autoSaveEnabled = enabled;
            GameLogger.LogInfo($"[AutoSaveManager] 자동 저장 {(enabled ? "활성화" : "비활성화")}", GameLogger.LogCategory.Save);
        }

        /// <summary>
        /// 마지막 저장 정보 반환
        /// </summary>
        public string GetLastSaveInfo()
        {
            return $"프레임: {lastSavedFrame}, 트리거: {lastSavedTrigger}";
        }

        #endregion
    }
}