using UnityEngine;
using System.Collections;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Audio
{
    /// <summary>
    /// 최소 오디오 시스템 (BGM + SFX만)
    /// </summary>
    public class AudioManager : MonoBehaviour, ICoreSystemInitializable
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("오디오 소스")]
        [SerializeField] private AudioSource bgmSource;  // 배경음악
        [SerializeField] private AudioSource sfxSource;  // 효과음
        
        [Header("오디오 설정")]
        [SerializeField] private float bgmVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1.0f;
        
        [Header("배경음악")]
        [SerializeField] private AudioClip mainMenuBGM;
        [SerializeField] private AudioClip battleBGM;
        
        // 초기화 상태
        public bool IsInitialized { get; private set; } = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 오디오 시스템 초기화
        /// </summary>
        private void InitializeAudio()
        {
            // BGM 소스 설정
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }
            
            // SFX 소스 설정
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
            
            Debug.Log("[AudioManager] 오디오 시스템 초기화 완료");
        }
        
        /// <summary>
        /// 배경음악 재생
        /// </summary>
        public void PlayBGM(AudioClip bgmClip)
        {
            if (bgmClip == null) return;
            
            bgmSource.clip = bgmClip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
            
            Debug.Log($"[AudioManager] 배경음악 재생: {bgmClip.name}");
        }
        
        /// <summary>
        /// 효과음 재생
        /// </summary>
        public void PlaySFX(AudioClip sfxClip)
        {
            if (sfxClip == null) return;
            
            sfxSource.PlayOneShot(sfxClip, sfxVolume);
            
            Debug.Log($"[AudioManager] 효과음 재생: {sfxClip.name}");
        }
        
        /// <summary>
        /// BGM 볼륨 설정
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            bgmSource.volume = bgmVolume;
            Debug.Log($"[AudioManager] BGM 볼륨 설정: {bgmVolume}");
        }
        
        /// <summary>
        /// SFX 볼륨 설정
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            Debug.Log($"[AudioManager] SFX 볼륨 설정: {sfxVolume}");
        }
        
        /// <summary>
        /// 메인 메뉴 BGM 재생
        /// </summary>
        public void PlayMainMenuBGM()
        {
            PlayBGM(mainMenuBGM);
        }
        
        /// <summary>
        /// 전투 BGM 재생
        /// </summary>
        public void PlayBattleBGM()
        {
            PlayBGM(battleBGM);
        }
        
        /// <summary>
        /// 현재 BGM 볼륨 반환
        /// </summary>
        public float BGMVolume => bgmVolume;
        
        /// <summary>
        /// 현재 SFX 볼륨 반환
        /// </summary>
        public float SFXVolume => sfxVolume;
        
        #region ICoreSystemInitializable 구현
        /// <summary>
        /// 시스템 초기화 수행
        /// </summary>
        public IEnumerator Initialize()
        {
            GameLogger.LogInfo("AudioManager 초기화 시작", GameLogger.LogCategory.Audio);
            
            // 오디오 소스 초기화
            InitializeAudio();
            
            // 초기화 완료
            IsInitialized = true;
            
            GameLogger.LogInfo("AudioManager 초기화 완료", GameLogger.LogCategory.Audio);
            yield return null;
        }
        
        /// <summary>
        /// 초기화 실패 시 호출
        /// </summary>
        public void OnInitializationFailed()
        {
            GameLogger.LogError("AudioManager 초기화 실패", GameLogger.LogCategory.Error);
            IsInitialized = false;
        }
        #endregion
    }
}
