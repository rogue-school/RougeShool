using UnityEngine;
using UnityEngine.UI;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Manager;
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
        
        // DI로 주입
        [Inject(Optional = true)] private ISceneTransitionManager sceneTransitionManager;
        [Inject(Optional = true)] private SettingsManager settingsManager;
        // SaveSystem 및 Statistics 제거됨
        
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
            // saveManager, sceneTransitionManager, settingsManager는 DI로 주입받음
            if (enableDebugLogging)
            {
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
                // 다시하기 플래그 설정 (SaveProgressBeforeSceneTransition에서 완전 종료 처리)
                PlayerPrefs.SetInt("RESTART_GAME_REQUESTED", 1);
                PlayerPrefs.Save();
                
                // Statistics 및 SaveSystem 제거됨
                
                // 세션 ID 초기화 (다시하기는 새 게임)
                PlayerPrefs.DeleteKey("CURRENT_SESSION_ID");
                
                // 새게임 플래그 설정 (스테이지에서 추가 초기화 수행)
                PlayerPrefs.SetInt("NEW_GAME_REQUESTED", 1);
                PlayerPrefs.SetInt("RESUME_REQUESTED", 0);
                PlayerPrefs.SetInt("START_STAGE_NUMBER", 1);
                PlayerPrefs.SetInt("START_ENEMY_INDEX", 0);
                PlayerPrefs.Save();
                
                // 메인 메뉴로 이동
                await sceneTransitionManager.TransitionToMainScene();
                
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
