using UnityEngine;
using UnityEngine.UI;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Save;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Statistics;
using Zenject;

namespace Game.CoreSystem.UI
{
    /// <summary>
    /// 시스템 기능 전용 설정창 UI 컨트롤러
    /// 이어하기, 진행도 초기화, 메인 메뉴, 게임 종료 기능 제공
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private Button goToMainButton;
        [SerializeField] private Button exitGameButton;
        
        // 직접 참조 (DI 대신)
        private ISaveManager saveManager;
        private ISceneTransitionManager sceneTransitionManager;
        private SettingsManager settingsManager;
        
        [Header("설정")]
        [SerializeField] private bool enableDebugLogging = false;
        
        private void Start()
        {
            // 매니저들 찾기
            FindManagers();
            InitializeUI();
        }
        
        /// <summary>
        /// 매니저들 찾기
        /// </summary>
        private void FindManagers()
        {
            saveManager = FindFirstObjectByType<SaveManager>();
            sceneTransitionManager = FindFirstObjectByType<SceneTransitionManager>();
            settingsManager = FindFirstObjectByType<SettingsManager>();
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo($"매니저 찾기 완료 - SaveManager: {saveManager != null}, SceneTransitionManager: {sceneTransitionManager != null}, SettingsManager: {settingsManager != null}", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
            
            if (resetProgressButton != null)
                resetProgressButton.onClick.AddListener(OnResetProgressClicked);
            
            if (goToMainButton != null)
                goToMainButton.onClick.AddListener(OnGoToMainClicked);
            
            if (exitGameButton != null)
                exitGameButton.onClick.AddListener(OnExitGameClicked);
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("시스템 설정창 UI 초기화 완료", GameLogger.LogCategory.UI);
            }
        }
        
        #region 이벤트 핸들러
        
        /// <summary>
        /// 설정창 닫기
        /// </summary>
        private void OnCloseButtonClicked()
        {
            settingsManager.CloseSettings();
        }
        
        /// <summary>
        /// 이어하기 (계속 진행하기) - 창 닫기
        /// </summary>
        private void OnContinueClicked()
        {
            try
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("이어하기 - 창 닫기", GameLogger.LogCategory.UI);
                }
                
                // 설정창 닫기 (게임 계속 진행)
                settingsManager.CloseSettings();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"이어하기 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 다시하기 (진행도 초기화 + 메인 메뉴로 돌아가기)
        /// </summary>
        private async void OnResetProgressClicked()
        {
            try
            {
                GameLogger.LogInfo("다시하기 시작 - 전체 게임 상태 초기화", GameLogger.LogCategory.UI);
                
                // 다시하기 플래그 설정 (SaveProgressBeforeSceneTransition에서 완전 종료 처리)
                PlayerPrefs.SetInt("RESTART_GAME_REQUESTED", 1);
                PlayerPrefs.Save();
                
                // 통계 세션 완전 종료 (다시하기는 완전 종료) - 게임 승리와 동일한 로직
                var gameSessionStatistics = UnityEngine.Object.FindFirstObjectByType<GameSessionStatistics>(UnityEngine.FindObjectsInactive.Include);
                var statisticsManager = UnityEngine.Object.FindFirstObjectByType<StatisticsManager>(UnityEngine.FindObjectsInactive.Include);
                
                if (gameSessionStatistics != null && statisticsManager != null)
                {
                    // 세션이 활성화되어 있으면 종료 처리 (완전 종료)
                    if (gameSessionStatistics.IsSessionActive)
                    {
                        gameSessionStatistics.EndSession(true); // 완전 종료
                        var sessionData = gameSessionStatistics.GetCurrentSessionData();
                        if (sessionData != null)
                        {
                            await statisticsManager.SaveSessionStatistics(sessionData);
                            gameSessionStatistics.MarkAsSaved();
                            GameLogger.LogInfo("다시하기 - 통계 세션 완전 종료 및 저장 완료", GameLogger.LogCategory.Save);
                        }
                    }
                    else
                    {
                        // 세션이 이미 종료되었어도 데이터가 있으면 저장 시도 (게임 승리와 동일)
                        var sessionData = gameSessionStatistics.GetCurrentSessionData();
                        if (sessionData != null)
                        {
                            await statisticsManager.SaveSessionStatistics(sessionData);
                            gameSessionStatistics.MarkAsSaved();
                            GameLogger.LogInfo("다시하기 - 세션이 이미 종료되었지만, 기존 세션 데이터를 저장했습니다. (완전 종료)", GameLogger.LogCategory.Save);
                        }
                    }
                }
                else
                {
                    if (gameSessionStatistics == null)
                    {
                        GameLogger.LogWarning("다시하기 - GameSessionStatistics를 찾을 수 없습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                    }
                    if (statisticsManager == null)
                    {
                        GameLogger.LogWarning("다시하기 - StatisticsManager를 찾을 수 없습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                    }
                }
                
                // 세션 ID 초기화 (다시하기는 새 게임)
                PlayerPrefs.DeleteKey("CURRENT_SESSION_ID");
                
                // 전체 게임 상태 초기화 (새게임과 동일한 로직)
                if (saveManager != null)
                {
                    saveManager.InitializeNewGame();
                    GameLogger.LogInfo("다시하기 - 게임 상태 초기화 완료", GameLogger.LogCategory.Save);
                }
                
                // 새게임 플래그 설정 (스테이지에서 추가 초기화 수행)
                PlayerPrefs.SetInt("NEW_GAME_REQUESTED", 1);
                PlayerPrefs.SetInt("RESUME_REQUESTED", 0);
                PlayerPrefs.SetInt("START_STAGE_NUMBER", 1);
                PlayerPrefs.SetInt("START_ENEMY_INDEX", 0);
                PlayerPrefs.Save();
                
                // 메인 메뉴로 이동
                await sceneTransitionManager.TransitionToMainScene();
                
                GameLogger.LogInfo("다시하기 완료", GameLogger.LogCategory.UI);
                settingsManager.CloseSettings();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"다시하기 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 메인 메뉴로 돌아가기
        /// </summary>
        private async void OnGoToMainClicked()
        {
            try
            {
                GameLogger.LogInfo("메인 메뉴로 이동", GameLogger.LogCategory.UI);
                
                await sceneTransitionManager.TransitionToMainScene();
                settingsManager.CloseSettings();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"메인 메뉴 이동 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        private void OnExitGameClicked()
        {
            try
            {
                GameLogger.LogInfo("게임 종료", GameLogger.LogCategory.UI);
                
                // 게임 종료
                Application.Quit();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"게임 종료 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        #endregion
    }
}
