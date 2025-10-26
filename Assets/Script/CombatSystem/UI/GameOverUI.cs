using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Save;

namespace Game.CombatSystem.UI
{
    /// <summary>
    /// 게임 오버 화면을 관리하는 UI 컨트롤러입니다.
    /// 플레이어 사망 시 호출되어 게임 오버 화면을 표시하고, 메인 메뉴로 이동 또는 재시작을 지원합니다.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("게임 오버 UI 요소")]
        [Tooltip("게임 오버 패널 (배경)")]
        [SerializeField] private GameObject panel;

        [Tooltip("게임 오버 텍스트")]
        [SerializeField] private TextMeshProUGUI gameOverText;

        [Tooltip("재시작 버튼")]
        [SerializeField] private Button restartButton;

        [Tooltip("메인 메뉴 버튼")]
        [SerializeField] private Button mainMenuButton;

        // 씬 전환 매니저
        private ISceneTransitionManager sceneTransitionManager;

        private void Start()
        {
            // 씬 전환 매니저 찾기
            FindSceneTransitionManager();
            InitializeUI();
        }

        /// <summary>
        /// 씬 전환 매니저를 찾습니다.
        /// </summary>
        private void FindSceneTransitionManager()
        {
            sceneTransitionManager = FindFirstObjectByType<SceneTransitionManager>();
            if (sceneTransitionManager != null)
            {
                GameLogger.LogInfo("[GameOverUI] SceneTransitionManager 찾기 완료", GameLogger.LogCategory.UI);
            }
            else
            {
                GameLogger.LogWarning("[GameOverUI] SceneTransitionManager를 찾을 수 없습니다", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// UI를 초기화합니다.
        /// </summary>
        private void InitializeUI()
        {
            // 기본적으로 패널 숨김
            if (panel != null)
            {
                panel.SetActive(false);
            }

            // 버튼 이벤트 연결
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            GameLogger.LogInfo("[GameOverUI] 게임 오버 UI 초기화 완료", GameLogger.LogCategory.UI);
        }

        /// <summary>
        /// 게임 오버 화면을 표시합니다.
        /// </summary>
        public void ShowGameOver()
        {
            if (panel != null)
            {
                panel.SetActive(true);
                GameLogger.LogInfo("[GameOverUI] 게임 오버 화면 표시", GameLogger.LogCategory.UI);
            }

            // 게임 오버 텍스트 설정
            if (gameOverText != null)
            {
                gameOverText.text = "게임 오버";
            }
        }

        /// <summary>
        /// 게임 오버 화면을 숨깁니다.
        /// </summary>
        public void HideGameOver()
        {
            if (panel != null)
            {
                panel.SetActive(false);
                GameLogger.LogInfo("[GameOverUI] 게임 오버 화면 숨김", GameLogger.LogCategory.UI);
            }
        }

        /// <summary>
        /// 재시작 버튼 클릭 처리
        /// </summary>
        private async void OnRestartClicked()
        {
            try
            {
                GameLogger.LogInfo("[GameOverUI] 재시작 버튼 클릭", GameLogger.LogCategory.UI);

                HideGameOver();

                // 세이브 매니저 찾기
                var saveManager = FindFirstObjectByType<SaveManager>();
                if (saveManager != null)
                {
                    saveManager.InitializeNewGame();
                    GameLogger.LogInfo("[GameOverUI] 게임 상태 초기화 완료", GameLogger.LogCategory.Save);
                }

                // 새게임 플래그 설정
                PlayerPrefs.SetInt("NEW_GAME_REQUESTED", 1);
                PlayerPrefs.SetInt("RESUME_REQUESTED", 0);
                PlayerPrefs.SetInt("START_STAGE_NUMBER", 1);
                PlayerPrefs.SetInt("START_ENEMY_INDEX", 0);
                PlayerPrefs.Save();

                // 씬 전환
                if (sceneTransitionManager != null)
                {
                    await sceneTransitionManager.TransitionToStageScene();
                }
                else
                {
                    GameLogger.LogWarning("[GameOverUI] SceneTransitionManager가 없습니다. 직접 씬 전환", GameLogger.LogCategory.UI);
                    UnityEngine.SceneManagement.SceneManager.LoadScene("StageScene");
                }

                GameLogger.LogInfo("[GameOverUI] 재시작 완료", GameLogger.LogCategory.UI);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"재시작 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }

        /// <summary>
        /// 메인 메뉴 버튼 클릭 처리
        /// </summary>
        private async void OnMainMenuClicked()
        {
            try
            {
                GameLogger.LogInfo("[GameOverUI] 메인 메뉴 버튼 클릭", GameLogger.LogCategory.UI);

                HideGameOver();

                // 메인 메뉴로 이동
                if (sceneTransitionManager != null)
                {
                    await sceneTransitionManager.TransitionToMainScene();
                }
                else
                {
                    GameLogger.LogWarning("[GameOverUI] SceneTransitionManager가 없습니다. 직접 씬 전환", GameLogger.LogCategory.UI);
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
                }

                GameLogger.LogInfo("[GameOverUI] 메인 메뉴 이동 완료", GameLogger.LogCategory.UI);
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"메인 메뉴 이동 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
    }
}

