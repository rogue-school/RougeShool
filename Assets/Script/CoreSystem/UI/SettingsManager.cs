using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Audio;
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
        [SerializeField] private bool enableDebugLogging = true;
        
        // 현재 활성화된 설정창 인스턴스
        private GameObject currentSettingsPanel;
        private Canvas currentCanvas;
        
        // 의존성 주입
        [Inject] private IAudioManager audioManager;
        // SaveSystem 제거됨
        [Inject] private ISceneTransitionManager sceneTransitionManager;
        [InjectOptional] private Canvas mainCanvas;
        
        // 설정창 상태
        
        /// <summary>
        /// 설정 매니저가 초기화되었는지 여부를 나타냅니다.
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 설정창이 현재 열려있는지 여부를 나타냅니다.
        /// </summary>
        public bool IsSettingsOpen => currentSettingsPanel != null && currentSettingsPanel.activeInHierarchy;
        
        // 이벤트
        public static event Action OnSettingsOpened;
        public static event Action OnSettingsClosed;
        
        private void Awake()
        {
            // 자동 초기화 시작
            StartCoroutine(AutoInitialize());
        }
        
        /// <summary>
        /// 자동 초기화 (CoreSystemInitializer와 독립적으로)
        /// </summary>
        /// <returns>초기화 코루틴</returns>
        private IEnumerator AutoInitialize()
        {
            if (IsInitialized) yield break;
            
            // 프리팹이 없으면 Addressables에서 로드
            if (settingsPanelPrefab == null)
            {
                var handle = Addressables.LoadAssetAsync<GameObject>("Prefab/SettingsPanel");
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    settingsPanelPrefab = handle.Result;
                }
                else
                {
                    GameLogger.LogWarning("설정창 프리팹이 없어 설정 기능을 일시 비활성화합니다.", GameLogger.LogCategory.UI);
                    if (handle.OperationException != null)
                    {
                        GameLogger.LogError($"Addressables 로드 오류: {handle.OperationException.Message}", GameLogger.LogCategory.Error);
                    }
                    // 프리팹 없이도 초기화는 완료로 간주
                    IsInitialized = true;
                    yield break;
                }
            }
            
            // 초기화 완료 대기 (한 프레임)
            yield return null;
            
            IsInitialized = true;
        }
        
        #region ICoreSystemInitializable 구현
        
        /// <summary>
        /// 시스템을 초기화합니다
        /// </summary>
        /// <returns>초기화 코루틴</returns>
        public IEnumerator Initialize()
        {
            // 이미 자동 초기화가 완료되었으면 스킵
            if (IsInitialized) yield break;
            
            // 자동 초기화와 동일한 로직 사용
            yield return StartCoroutine(AutoInitialize());
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
                // 강제로 닫기
                CloseSettings();
            }
            
            // 프리팹이 없으면 기능 비활성화
            if (settingsPanelPrefab == null)
            {
                GameLogger.LogWarning("설정창 프리팹이 없어 설정창을 열 수 없습니다.", GameLogger.LogCategory.UI);
                return;
            }

            // 주입된 Canvas 사용(선택 주입). 없으면 탐색 시도
            currentCanvas = mainCanvas != null ? mainCanvas : FindStageCanvas();
            if (currentCanvas == null)
            {
                GameLogger.LogWarning("Canvas를 찾지 못해 설정창을 열 수 없습니다.", GameLogger.LogCategory.UI);
                return;
            }
            
            if (enableDebugLogging)
            {
            }
            // 설정창 생성
            currentSettingsPanel = Instantiate(settingsPanelPrefab, currentCanvas.transform);
            currentSettingsPanel.name = "SettingsPanel";
            
            // 설정창 강제 활성화
            currentSettingsPanel.SetActive(true);
            
            // 프리팹에 Canvas가 없으면 자동으로 추가하고 최상단 정렬을 보장
            var settingsCanvas = currentSettingsPanel.GetComponent<Canvas>();
            if (settingsCanvas == null)
            {
                settingsCanvas = currentSettingsPanel.AddComponent<Canvas>();
            }
            settingsCanvas.overrideSorting = true;
            settingsCanvas.sortingOrder = 5000; // 다른 UI 위로 고정

            // 클릭을 위해 GraphicRaycaster 보장
            if (currentSettingsPanel.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                currentSettingsPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // 설정창을 최상위로 이동 (Sibling Index 최대값으로 설정)
            currentSettingsPanel.transform.SetAsLastSibling();
            
            // 설정창 활성화 확인
            if (enableDebugLogging)
            {
            }
            // 설정창 초기화
            InitializeSettingsPanel();
            
            // 이벤트 발생
            OnSettingsOpened?.Invoke();
            
            if (enableDebugLogging)
            {
            }
        }
        
        /// <summary>
        /// 설정창 닫기
        /// </summary>
        public void CloseSettings()
        {
            if (!IsSettingsOpen)
            {
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
            
            // SettingsPanelController 찾기
            var settingsController = currentSettingsPanel.GetComponent<SettingsPanelController>();
                if (settingsController == null)
                {
                    // SettingsPanelController가 없으면 추가
                    settingsController = currentSettingsPanel.AddComponent<SettingsPanelController>();
                    
                    if (enableDebugLogging)
                    {
                    }
                }
            
            // 간단한 닫기 버튼 연결 (fallback)
            var closeButton = currentSettingsPanel.GetComponentInChildren<Button>();
            if (closeButton != null)
            {
                // 기존 이벤트 제거 후 새로 연결
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseSettings);
                
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo($"닫기 버튼 연결 완료: {closeButton.name}", GameLogger.LogCategory.UI);
                }
            }
            
            // 오디오 설정 초기화
            InitializeAudioSettings();
            
            // 게임 설정 초기화
            InitializeGameSettings();
            
            if (enableDebugLogging)
            {
                GameLogger.LogInfo("설정창 초기화 완료", GameLogger.LogCategory.UI);
            }
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
            GameLogger.LogInfo("[SettingsManager] 게임 종료 요청", GameLogger.LogCategory.UI);
            
            // Unity 에디터와 빌드 환경 구분
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        #endregion
        
        #region Canvas 찾기
        
        /// <summary>
        /// 스테이지의 Canvas를 우선적으로 찾기
        /// </summary>
        private Canvas FindStageCanvas()
        {
            // 모든 Canvas 찾기
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            
            // 우선순위: 스테이지 Canvas > 일반 Canvas > CoreUI Canvas
            foreach (Canvas canvas in allCanvases)
            {
                // DontDestroyOnLoad가 아닌 Canvas 우선 선택
                if (!IsDontDestroyOnLoadCanvas(canvas))
                {
                    if (enableDebugLogging)
                    {
                        GameLogger.LogInfo($"스테이지 Canvas 발견: {canvas.name}", GameLogger.LogCategory.UI);
                    }
                    return canvas;
                }
            }
            
            // 스테이지 Canvas가 없으면 첫 번째 Canvas 사용
            if (allCanvases.Length > 0)
            {
                if (enableDebugLogging)
                {
                    GameLogger.LogInfo($"스테이지 Canvas가 없어 첫 번째 Canvas 사용: {allCanvases[0].name}", GameLogger.LogCategory.UI);
                }
                return allCanvases[0];
            }
            
            return null;
        }
        
        /// <summary>
        /// Canvas가 DontDestroyOnLoad인지 확인
        /// </summary>
        private bool IsDontDestroyOnLoadCanvas(Canvas canvas)
        {
            // DontDestroyOnLoad 오브젝트는 씬에 없고 DontDestroyOnLoad 씬에 있음
            return canvas.gameObject.scene.name == "DontDestroyOnLoad";
        }
        
        #endregion
        
    }
}
