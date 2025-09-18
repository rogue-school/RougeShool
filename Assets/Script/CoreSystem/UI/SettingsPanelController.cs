using UnityEngine;
using UnityEngine.UI;
using Game.CoreSystem.Interface;
using Zenject;

namespace Game.CoreSystem.UI
{
    /// <summary>
    /// 설정창 UI 컨트롤러
    /// 설정창 프리팹에 붙이는 스크립트
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private Button goToMainButton;
        
        // 의존성 주입
        [Inject] private IAudioManager audioManager;
        [Inject] private ISaveManager saveManager;
        [Inject] private ISceneTransitionManager sceneTransitionManager;
        [Inject] private SettingsManager settingsManager;
        [SerializeField] private Button exitGameButton;
        
        [Header("설정")]
        [SerializeField] private bool enableDebugLogging = false;
        
        private void Start()
        {
            // 저장된 오디오 설정을 먼저 로드하여 반영
            var (bgm, sfx) = saveManager.LoadAudioSettings(audioManager.BgmVolume, audioManager.SfxVolume);
            audioManager.SetBGMVolume(bgm);
            audioManager.SetSFXVolume(sfx);
            
            InitializeUI();
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            
            if (resetProgressButton != null)
                resetProgressButton.onClick.AddListener(OnResetProgressClicked);
            
            if (goToMainButton != null)
                goToMainButton.onClick.AddListener(OnGoToMainClicked);
            
            if (exitGameButton != null)
                exitGameButton.onClick.AddListener(OnExitGameClicked);
            
            // 슬라이더 초기값 설정
            if (bgmSlider != null)
            {
                bgmSlider.value = audioManager.BgmVolume;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            
            if (sfxSlider != null)
            {
                sfxSlider.value = audioManager.SfxVolume;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            if (enableDebugLogging)
            {
                Debug.Log("[SettingsPanelController] 설정창 UI 초기화 완료");
            }
        }
        
        #region 이벤트 핸들러
        
        private void OnCloseButtonClicked()
        {
            settingsManager.CloseSettings();
        }
        
        private void OnBGMVolumeChanged(float volume)
        {
            audioManager.SetBGMVolume(volume);
            saveManager.SaveAudioSettings(audioManager.BgmVolume, audioManager.SfxVolume);
        }
        
        private void OnSFXVolumeChanged(float volume)
        {
            audioManager.SetSFXVolume(volume);
            saveManager.SaveAudioSettings(audioManager.BgmVolume, audioManager.SfxVolume);
        }
        
        private async void OnResetProgressClicked()
        {
            // SaveManager에 ResetSaveData 메서드가 없으므로 주석 처리
            // saveManager.ResetSaveData();
            await sceneTransitionManager.TransitionToMainScene();
            settingsManager.CloseSettings();
        }
        
        private async void OnGoToMainClicked()
        {
            await sceneTransitionManager.TransitionToMainScene();
            settingsManager.CloseSettings();
        }
        
        private void OnExitGameClicked()
        {
            Application.Quit();
        }
        
        #endregion
    }
}
