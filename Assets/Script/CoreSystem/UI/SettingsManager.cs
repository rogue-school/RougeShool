using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Save;
using Game.CoreSystem.Utility;
using Zenject;

namespace Game.CoreSystem.UI
{
    /// <summary>
    /// 전역 설정창 관리자
    /// 어디서든 설정창을 열고 닫을 수 있는 중앙 관리 시스템
    /// </summary>
    public class SettingsManager : MonoBehaviour, ICoreSystemInitializable
    {
        [Header("설정창 프리팹")]
        [SerializeField] private GameObject settingsPanelPrefab;
        
        [Header("설정창 관리")]
        [SerializeField] private bool enableDebugLogging = false;
        
        // 현재 활성화된 설정창 인스턴스
        private GameObject currentSettingsPanel;
        private Canvas currentCanvas;
        
        // 의존성 주입
        [Inject] private IAudioManager audioManager;
        [Inject] private ISaveManager saveManager;
        [Inject] private ISceneTransitionManager sceneTransitionManager;
        [InjectOptional] private Canvas mainCanvas;
        
        // 설정창 상태
        public bool IsInitialized { get; private set; }
        public bool IsSettingsOpen => currentSettingsPanel != null && currentSettingsPanel.activeInHierarchy;
        
        // 이벤트
        public static event Action OnSettingsOpened;
        public static event Action OnSettingsClosed;
        
        #region ICoreSystemInitializable 구현
        
        public IEnumerator Initialize()
        {
            if (IsInitialized) yield break;
            
            // 프리팹이 없으면 Resources에서 로드 (없어도 기능을 일시 비활성화하고 계속 진행)
            if (settingsPanelPrefab == null)
            {
                settingsPanelPrefab = Resources.Load<GameObject>("Prefab/SettingsPanel");
                if (settingsPanelPrefab == null)
                {
                    GameLogger.LogWarning("설정창 프리팹이 없어 설정 기능을 일시 비활성화합니다.", GameLogger.LogCategory.UI);
                    // 프리팹 없이도 초기화는 완료로 간주하여 다른 시스템 진행을 막지 않음
                    IsInitialized = true;
                    yield break;
                }
            }
            
            // 초기화 완료 대기 (한 프레임)
            yield return null;
            
            IsInitialized = true;
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("SettingsManager 초기화 완료", GameLogger.LogCategory.UI);
            }
        }
        
        public void OnInitializationFailed()
        {
            GameLogger.LogError("SettingsManager 초기화 실패", GameLogger.LogCategory.Error);
        }
        
        #endregion
        
        #region 설정창 제어
        
        /// <summary>
        /// 설정창 열기 (어디서든 호출 가능)
        /// </summary>
        public void OpenSettings()
        {
            if (!IsInitialized)
            {
                GameLogger.LogError("SettingsManager가 초기화되지 않았습니다", GameLogger.LogCategory.Error);
                return;
            }
            
            if (IsSettingsOpen)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("설정창이 이미 열려있습니다", GameLogger.LogCategory.UI);
                }
                return;
            }
            
            // 프리팹이 없으면 기능 비활성화
            if (settingsPanelPrefab == null)
            {
                GameLogger.LogWarning("설정창 프리팹이 없어 설정창을 열 수 없습니다.", GameLogger.LogCategory.UI);
                return;
            }

            // 주입된 Canvas 사용(선택 주입). 없으면 탐색 시도
            currentCanvas = mainCanvas != null ? mainCanvas : FindFirstObjectByType<Canvas>();
            if (currentCanvas == null)
            {
                GameLogger.LogWarning("Canvas를 찾지 못해 설정창을 열 수 없습니다.", GameLogger.LogCategory.UI);
                return;
            }
            
            // 설정창 생성
            currentSettingsPanel = Instantiate(settingsPanelPrefab, currentCanvas.transform);
            currentSettingsPanel.name = "SettingsPanel";
            
            // 설정창 초기화
            InitializeSettingsPanel();
            
            // 이벤트 발생
            OnSettingsOpened?.Invoke();
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("설정창 열기 완료", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 설정창 닫기
        /// </summary>
        public void CloseSettings()
        {
            if (!IsSettingsOpen)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo("설정창이 이미 닫혀있습니다", GameLogger.LogCategory.UI);
                }
                return;
            }
            
            // 설정창 제거
            Destroy(currentSettingsPanel);
            currentSettingsPanel = null;
            currentCanvas = null;
            
            // 이벤트 발생
            OnSettingsClosed?.Invoke();
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("설정창 닫기 완료", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 설정창 토글 (열기/닫기)
        /// </summary>
        public void ToggleSettings()
        {
            if (IsSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
        
        #endregion
        
        #region 설정창 초기화
        
        /// <summary>
        /// 설정창 UI 요소 초기화
        /// </summary>
        private void InitializeSettingsPanel()
        {
            if (currentSettingsPanel == null) return;
            
            // 설정창의 버튼들 찾기 및 이벤트 연결
            var closeButton = currentSettingsPanel.GetComponentInChildren<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseSettings);
            }
            
            // 오디오 설정 초기화
            InitializeAudioSettings();
            
            // 게임 설정 초기화
            InitializeGameSettings();
        }
        
        /// <summary>
        /// 오디오 설정 초기화
        /// </summary>
        private void InitializeAudioSettings()
        {
            // BGM 슬라이더
            var bgmSlider = currentSettingsPanel.transform.Find("AudioSettings/BGMSlider")?.GetComponent<Slider>();
            if (bgmSlider != null)
            {
                bgmSlider.value = audioManager.BgmVolume;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            
            // SFX 슬라이더
            var sfxSlider = currentSettingsPanel.transform.Find("AudioSettings/SFXSlider")?.GetComponent<Slider>();
            if (sfxSlider != null)
            {
                sfxSlider.value = audioManager.SfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }
        
        /// <summary>
        /// 게임 설정 초기화
        /// </summary>
        private void InitializeGameSettings()
        {
            // 게임 진행 초기화 버튼
            var resetProgressButton = currentSettingsPanel.transform.Find("GameSettings/ResetProgressButton")?.GetComponent<Button>();
            if (resetProgressButton != null)
            {
                resetProgressButton.onClick.AddListener(OnResetProgressClicked);
            }
            
            // 메인 화면으로 이동 버튼
            var goToMainButton = currentSettingsPanel.transform.Find("GameSettings/GoToMainButton")?.GetComponent<Button>();
            if (goToMainButton != null)
            {
                goToMainButton.onClick.AddListener(OnGoToMainClicked);
            }
            
            // 게임 종료 버튼
            var exitGameButton = currentSettingsPanel.transform.Find("GameSettings/ExitGameButton")?.GetComponent<Button>();
            if (exitGameButton != null)
            {
                exitGameButton.onClick.AddListener(OnExitGameClicked);
            }
        }
        
        #endregion
        
        #region 설정 이벤트 핸들러
        
        /// <summary>
        /// BGM 볼륨 변경
        /// </summary>
        private void OnBGMVolumeChanged(float volume)
        {
            audioManager.SetBGMVolume(volume);
        }
        
        /// <summary>
        /// SFX 볼륨 변경
        /// </summary>
        private void OnSFXVolumeChanged(float volume)
        {
            audioManager.SetSFXVolume(volume);
        }
        
        /// <summary>
        /// 게임 진행 초기화
        /// </summary>
        private async void OnResetProgressClicked()
        {
            // SaveManager에 ResetSaveData 메서드가 없으므로 주석 처리
            // saveManager.ResetSaveData();
            await sceneTransitionManager.TransitionToMainScene();
            CloseSettings();
        }
        
        /// <summary>
        /// 메인 화면으로 이동
        /// </summary>
        private async void OnGoToMainClicked()
        {
            await sceneTransitionManager.TransitionToMainScene();
            CloseSettings();
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        private void OnExitGameClicked()
        {
            Application.Quit();
        }
        
        #endregion
        
    }
}
