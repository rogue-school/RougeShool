using UnityEngine;
using UnityEngine.UI;
using Game.CoreSystem.UI;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.StageSystem.UI
{
    /// <summary>
    /// 스테이지 씬 UI 컨트롤러
    /// 스테이지의 설정 버튼과 기타 UI 요소들을 관리
    /// </summary>
    public class StageUIController : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Button settingsButton;  // 스테이지의 설정 버튼
        
        [Header("설정")]
        [SerializeField] private bool enableDebugLogging = true;

        // SettingsManager DI 주입
        [Inject(Optional = true)] private SettingsManager settingsManager;

        private void Start()
        {
            InitializeUI();

            if (settingsManager == null && enableDebugLogging)
            {
                GameLogger.LogWarning("SettingsManager가 주입되지 않았습니다.", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 설정 버튼 이벤트 연결
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
                
                if (enableDebugLogging)
                {
                    Debug.Log("[StageUIController] 설정 버튼 연결 완료");
                }
            }
            else
            {
                Debug.LogWarning("[StageUIController] 설정 버튼이 연결되지 않았습니다!");
            }
        }
        
        private void Update()
        {
            // ESC 키로 설정창 열기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnSettingsButtonClicked();
            }
        }
        
        /// <summary>
        /// 설정 버튼 클릭 시 설정창 열기
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            try
            {
                if (settingsManager == null)
                {
                    GameLogger.LogError("SettingsManager가 null입니다. CoreScene이 로드되었는지 확인하세요.", GameLogger.LogCategory.Error);
                    return;
                }
                
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("설정창 열기 요청", GameLogger.LogCategory.UI);
                }
                
                settingsManager.OpenSettings();
            }
            catch (System.Exception ex)
            {
                GameLogger.LogError($"설정창 열기 실패: {ex.Message}", GameLogger.LogCategory.Error);
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 해제
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        }
    }
}
