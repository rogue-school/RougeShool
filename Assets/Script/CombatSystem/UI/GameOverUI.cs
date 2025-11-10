using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.CoreSystem.Utility;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Save;
using Game.CoreSystem.Statistics;
using Zenject;

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

        // 재시작 버튼은 사용하지 않음 (디자인에 따라 비활성/숨김)
        [SerializeField] private Button restartButton;

        [Tooltip("메인 메뉴 버튼")]
        [SerializeField] private Button mainMenuButton;

        // 씬 전환 매니저
        private ISceneTransitionManager sceneTransitionManager;
        
        // 통계 매니저
        [Inject(Optional = true)] private GameSessionStatistics gameSessionStatistics;
        [Inject(Optional = true)] private IStatisticsManager statisticsManager;

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
            // 재시작 버튼 제거 요구: 버튼이 있다면 비활성화/숨김 처리
            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(false);
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
        public async void ShowGameOver()
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

            // 통계 세션 종료 및 저장
            await SaveStatisticsSession();
        }

        /// <summary>
        /// 통계 세션 저장 (게임 승리와 동일한 로직)
        /// </summary>
        private async System.Threading.Tasks.Task SaveStatisticsSession()
        {
            GameLogger.LogInfo("[GameOverUI] 통계 세션 저장 시도 (완전 종료)", GameLogger.LogCategory.Save);

            if (gameSessionStatistics == null)
            {
                gameSessionStatistics = FindFirstObjectByType<GameSessionStatistics>(FindObjectsInactive.Include);
                GameLogger.LogInfo($"[GameOverUI] GameSessionStatistics 찾기: {(gameSessionStatistics != null ? "성공" : "실패")}", GameLogger.LogCategory.Save);
            }

            if (statisticsManager == null)
            {
                statisticsManager = FindFirstObjectByType<StatisticsManager>(FindObjectsInactive.Include);
                GameLogger.LogInfo($"[GameOverUI] StatisticsManager 찾기: {(statisticsManager != null ? "성공" : "실패")}", GameLogger.LogCategory.Save);
            }

            if (gameSessionStatistics == null)
            {
                GameLogger.LogWarning("[GameOverUI] GameSessionStatistics를 찾을 수 없습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                return;
            }

            if (statisticsManager == null)
            {
                GameLogger.LogWarning("[GameOverUI] StatisticsManager를 찾을 수 없습니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                return;
            }

            // 게임 승리와 동일하게 강제로 완전 종료 및 저장 (IsSaved 체크 제거)
            // 세션이 활성화되어 있으면 종료 처리 (완전 종료)
            if (gameSessionStatistics.IsSessionActive)
            {
                gameSessionStatistics.EndSession(true); // 완전 종료
                var sessionData = gameSessionStatistics.GetCurrentSessionData();
                
                if (sessionData == null)
                {
                    GameLogger.LogWarning("[GameOverUI] 세션 종료 후 데이터가 null입니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                    return;
                }
                
                await statisticsManager.SaveSessionStatistics(sessionData);
                gameSessionStatistics.MarkAsSaved();
                GameLogger.LogInfo("[GameOverUI] 통계 세션 저장 완료 (완전 종료)", GameLogger.LogCategory.Save);
            }
            else
            {
                // 세션이 이미 종료되었어도 데이터가 있으면 저장 시도 (게임 승리와 동일)
                var sessionData = gameSessionStatistics.GetCurrentSessionData();
                if (sessionData != null)
                {
                    await statisticsManager.SaveSessionStatistics(sessionData);
                    gameSessionStatistics.MarkAsSaved();
                    GameLogger.LogInfo("[GameOverUI] 세션이 이미 종료되었지만, 기존 세션 데이터를 저장했습니다. (완전 종료)", GameLogger.LogCategory.Save);
                }
                else
                {
                    GameLogger.LogWarning("[GameOverUI] 세션 데이터가 null입니다. 통계 저장을 건너뜁니다.", GameLogger.LogCategory.Save);
                }
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

        // 재시작 로직은 미사용 (요구사항에 따라 제거)

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


