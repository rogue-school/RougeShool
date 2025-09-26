using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Game.CoreSystem.Interface;
using Game.CoreSystem.UI;
using Game.CoreSystem.Save;
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
        [SerializeField] private Button continueButton;
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
        [Inject] private ISaveManager saveManager;
        
        private void Start()
        {
            Debug.Log("[MainSceneController] ===== 초기화 시작 =====");
            
            // 의존성 주입 상태 확인
            Debug.Log($"[MainSceneController] 의존성 주입 상태:");
            Debug.Log($"  - GameStateManager: {gameStateManager != null}");
            Debug.Log($"  - SettingsManager: {settingsManager != null}");
            Debug.Log($"  - SaveManager: {saveManager != null}");
            
            // UI 요소 연결 상태 확인
            Debug.Log($"[MainSceneController] UI 요소 연결 상태:");
            Debug.Log($"  - StartButton: {startButton != null}");
            Debug.Log($"  - ContinueButton: {continueButton != null}");
            Debug.Log($"  - SettingsButton: {settingsButton != null}");
            Debug.Log($"  - ExitButton: {exitButton != null}");
            
            InitializeUI();
            LoadSettings();
            UpdateContinueButtonState();
            
            Debug.Log("[MainSceneController] ===== 초기화 완료 =====");
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonClicked);
            
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueButtonClicked);
            
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
        /// 이어하기 버튼 상태 업데이트
        /// </summary>
        private void UpdateContinueButtonState()
        {
            if (continueButton != null)
            {
                // 스테이지 진행 저장 파일이 있는지 확인
                bool hasStageProgressSave = saveManager?.HasStageProgressSave() ?? false;
                continueButton.interactable = hasStageProgressSave;
                
                Debug.Log($"[MainSceneController] 이어하기 버튼 상태 업데이트:");
                Debug.Log($"  - SaveManager 존재: {saveManager != null}");
                Debug.Log($"  - 스테이지 진행 저장 파일 존재: {hasStageProgressSave}");
                Debug.Log($"  - 버튼 활성화: {continueButton.interactable}");
                
                // 버튼 텍스트도 업데이트 (선택사항)
                var buttonText = continueButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "이어하기";
                    buttonText.color = hasStageProgressSave ? Color.white : Color.gray;
                }
            }
            else
            {
                Debug.LogWarning("[MainSceneController] 이어하기 버튼이 연결되지 않았습니다!");
            }
        }
        
        /// <summary>
        /// 게임 시작 버튼 클릭
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("[MainSceneController] 새 게임 시작");
            
            try
            {
                // 1. 기존 저장 데이터 완전 초기화
                if (saveManager != null)
                {
                    saveManager.InitializeNewGame();
                    Debug.Log("[MainSceneController] 기존 저장 데이터 초기화 완료");
                }
                
                // 2. 캐릭터 선택 UI 활성화 (씬 전환은 캐릭터 선택 후에만 발생)
                // TODO: 캐릭터 선택 UI 활성화 로직 추가
                Debug.Log("[MainSceneController] 캐릭터 선택 UI 활성화 필요");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainSceneController] 새 게임 시작 실패: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 이어하기 버튼 클릭
        /// </summary>
        private async void OnContinueButtonClicked()
        {
            Debug.Log("[MainSceneController] ===== 이어하기 버튼 클릭 시작 =====");
            
            // 기본 상태 확인
            Debug.Log($"[MainSceneController] 기본 상태 확인:");
            Debug.Log($"  - SaveManager: {saveManager != null}");
            Debug.Log($"  - ContinueButton: {continueButton != null}");
            Debug.Log($"  - ContinueButton.interactable: {continueButton?.interactable}");
            
            if (saveManager == null)
            {
                Debug.LogError("[MainSceneController] ❌ SaveManager가 없습니다!");
                Debug.LogError("  - MainSceneInstaller에서 ISaveManager 바인딩 확인 필요");
                Debug.LogError("  - CoreScene에서 SaveManager가 생성되었는지 확인 필요");
                return;
            }
            
            if (continueButton == null)
            {
                Debug.LogError("[MainSceneController] ❌ 이어하기 버튼이 연결되지 않았습니다!");
                Debug.LogError("  - Inspector에서 Continue Button 필드 연결 확인 필요");
                return;
            }
            
            if (!continueButton.interactable)
            {
                Debug.LogWarning("[MainSceneController] ⚠️ 이어하기 버튼이 비활성화 상태입니다!");
                Debug.LogWarning("  - 저장 파일이 없거나 버튼 상태 업데이트 실패");
                return;
            }
            
            try
            {
                Debug.Log("[MainSceneController] 스테이지 진행 저장 파일 존재 여부 재확인...");
                bool hasStageProgressSave = saveManager.HasStageProgressSave();
                Debug.Log($"  - 스테이지 진행 저장 파일 존재: {hasStageProgressSave}");
                
                if (!hasStageProgressSave)
                {
                    Debug.LogWarning("[MainSceneController] ⚠️ 스테이지 진행 저장 파일이 존재하지 않습니다!");
                    Debug.LogWarning("  - 버튼 상태와 실제 저장 파일 상태가 일치하지 않음");
                    return;
                }
                
                Debug.Log("[MainSceneController] 스테이지 진행 상황 로드 시작...");

                // 1) 오디오 설정 로드
                var (bgm, sfx) = saveManager.LoadAudioSettings();

                // 2) 이어하기 재개 플래그 설정(초기 셋업 우회)
                PlayerPrefs.SetInt("RESUME_REQUESTED", 1);
                PlayerPrefs.Save();

                // 3) 스테이지 씬으로 전환
                var sceneLoader = FindFirstObjectByType<Game.CoreSystem.Manager.SceneTransitionManager>();
                if (sceneLoader == null)
                {
                    Debug.LogError("[MainSceneController] 씬 전환 매니저가 없습니다.");
                    return;
                }

                await sceneLoader.TransitionToStageScene();

                // 4) 씬 전환 완료 후 스테이지 진행 상황 복원
                bool loadSuccess = await saveManager.LoadStageProgress();
                Debug.Log($"[MainSceneController] 스테이지 진행 상황 로드 결과: {loadSuccess}");

                if (loadSuccess)
                {
                    Debug.Log("[MainSceneController] ✅ 스테이지 진행 상황 복원 성공!");
                }
                else
                {
                    Debug.LogWarning("[MainSceneController] ⚠️ 스테이지 진행 상황 복원 실패!");
                    Debug.LogWarning("  - 새 게임으로 폴백");
                    // 로드 실패 시 새 게임 시작
                    OnStartButtonClicked();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainSceneController] ❌ 이어하기 중 오류 발생: {ex.Message}");
                Debug.LogError($"  - 스택 트레이스: {ex.StackTrace}");
                // 오류 발생 시 새 게임 시작
                OnStartButtonClicked();
            }
            
            Debug.Log("[MainSceneController] ===== 이어하기 버튼 클릭 완료 =====");
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
