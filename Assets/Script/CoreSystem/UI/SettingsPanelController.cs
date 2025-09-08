using UnityEngine;
using UnityEngine.UI;
using Game.CoreSystem.Manager;
using Game.CoreSystem.Audio;
using Game.CoreSystem.Save;

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
        [SerializeField] private Button exitGameButton;
        
        [Header("설정")]
        [SerializeField] private bool enableDebugLogging = false;
        
        private void Start()
        {
            // 저장된 오디오 설정을 먼저 로드하여 반영
            var (bgm, sfx) = SaveManager.Instance.LoadAudioSettings(AudioManager.Instance.BGMVolume, AudioManager.Instance.SFXVolume);
            AudioManager.Instance.SetBGMVolume(bgm);
            AudioManager.Instance.SetSFXVolume(sfx);
            
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
                bgmSlider.value = AudioManager.Instance.BGMVolume;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            
            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.SFXVolume;
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
            SettingsManager.Instance.CloseSettings();
        }
        
        private void OnBGMVolumeChanged(float volume)
        {
            AudioManager.Instance.SetBGMVolume(volume);
            SaveManager.Instance.SaveAudioSettings(AudioManager.Instance.BGMVolume, AudioManager.Instance.SFXVolume);
        }
        
        private void OnSFXVolumeChanged(float volume)
        {
            AudioManager.Instance.SetSFXVolume(volume);
            SaveManager.Instance.SaveAudioSettings(AudioManager.Instance.BGMVolume, AudioManager.Instance.SFXVolume);
        }
        
        private async void OnResetProgressClicked()
        {
            SaveManager.Instance.ResetSaveData();
            await SceneTransitionManager.Instance.TransitionToMainScene();
            SettingsManager.Instance.CloseSettings();
        }
        
        private async void OnGoToMainClicked()
        {
            await SceneTransitionManager.Instance.TransitionToMainScene();
            SettingsManager.Instance.CloseSettings();
        }
        
        private void OnExitGameClicked()
        {
            Application.Quit();
        }
        
        #endregion
    }
}
