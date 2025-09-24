using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Game.CoreSystem.Interface;
using Game.CoreSystem.UI;
using Zenject;

namespace Game.UISystem
{
    /// <summary>
    /// 메인 씬 컨트롤러 (설정창 포함)
    /// </summary>
    public class MainSceneController : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        
        [Header("설정창")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Button closeSettingsButton;
        
        // 의존성 주입
        [Inject] private IGameStateManager gameStateManager;
        [Inject] private SettingsManager settingsManager;
        
        private void Start()
        {
            InitializeUI();
            LoadSettings();
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitButtonClicked);
            
            if (closeSettingsButton != null)
                closeSettingsButton.onClick.AddListener(OnCloseSettingsButtonClicked);
            
            // 슬라이더 이벤트 연결
            if (bgmVolumeSlider != null)
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            
            // 설정창 초기 상태
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
        
        /// <summary>
        /// 설정 로드
        /// </summary>
        private void LoadSettings()
        {
            // 기본값 설정
            if (bgmVolumeSlider != null)
                bgmVolumeSlider.value = 0.7f;
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = 1.0f;
        }
        
        /// <summary>
        /// 게임 시작 버튼 클릭
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("[MainSceneController] 게임 시작");
            
            // 캐릭터 선택 UI 활성화 (씬 전환은 캐릭터 선택 후에만 발생)
            // TODO: 캐릭터 선택 UI 활성화 로직 추가
        }
        
        /// <summary>
        /// 설정 버튼 클릭
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("[MainSceneController] 설정창 열기");
            settingsManager.OpenSettings();
        }
        
        /// <summary>
        /// 게임 종료 버튼 클릭
        /// </summary>
        private void OnExitButtonClicked()
        {
            Debug.Log("[MainSceneController] 게임 종료");
            gameStateManager.ExitGame();
        }
        
        /// <summary>
        /// 설정창 닫기 버튼 클릭
        /// </summary>
        private void OnCloseSettingsButtonClicked()
        {
            Debug.Log("[MainSceneController] 설정창 닫기");
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
        
        /// <summary>
        /// BGM 볼륨 변경 (현재 사용하지 않음 - SettingsManager에서 처리)
        /// </summary>
        private void OnBGMVolumeChanged(float value)
        {
            // 오디오 설정은 SettingsManager에서 처리
            Debug.Log($"[MainSceneController] BGM 볼륨 변경: {value}");
        }
        
        /// <summary>
        /// SFX 볼륨 변경 (현재 사용하지 않음 - SettingsManager에서 처리)
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            // 오디오 설정은 SettingsManager에서 처리
            Debug.Log($"[MainSceneController] SFX 볼륨 변경: {value}");
        }
    }
}
