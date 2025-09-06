using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Game.CoreSystem.Interface;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Audio
{
    /// <summary>
    /// 체계화된 오디오 시스템 (씬별 BGM + SFX 관리)
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
        [SerializeField] private float fadeTime = 1.0f;  // 페이드 시간
        
        [Header("씬별 배경음악")]
        [SerializeField] private AudioClip mainMenuBGM;
        [SerializeField] private AudioClip battleBGM;
        [SerializeField] private AudioClip shopBGM;
        [SerializeField] private AudioClip inventoryBGM;
        
        // 씬별 BGM 매핑
        private Dictionary<string, AudioClip> sceneBGMMap;
        
        // 현재 재생 중인 BGM
        private AudioClip currentBGM;
        private bool isFading = false;
        
        // 초기화 상태
        public bool IsInitialized { get; private set; } = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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
            
            // 씬별 BGM 매핑 초기화
            InitializeSceneBGMMap();
            
            // 씬 전환 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Debug.Log("[AudioManager] 오디오 시스템 초기화 완료");
        }
        
        /// <summary>
        /// 씬별 BGM 매핑 초기화
        /// </summary>
        private void InitializeSceneBGMMap()
        {
            sceneBGMMap = new Dictionary<string, AudioClip>
            {
                { "MainScene", mainMenuBGM },
                { "BattleScene", battleBGM },
                { "ShopScene", shopBGM },
                { "InventoryScene", inventoryBGM }
            };
        }
        
        /// <summary>
        /// 씬 로드 시 자동으로 BGM 변경
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (sceneBGMMap.ContainsKey(scene.name))
            {
                PlaySceneBGM(scene.name);
            }
        }
        
        /// <summary>
        /// 배경음악 재생 (페이드 효과 포함)
        /// </summary>
        public void PlayBGM(AudioClip bgmClip, bool fadeIn = true)
        {
            if (bgmClip == null) return;
            
            // 같은 BGM이면 재생하지 않음
            if (currentBGM == bgmClip && bgmSource.isPlaying) return;
            
            if (fadeIn && bgmSource.isPlaying)
            {
                StartCoroutine(FadeToNewBGM(bgmClip));
            }
            else
            {
                bgmSource.clip = bgmClip;
                bgmSource.volume = bgmVolume;
                bgmSource.Play();
                currentBGM = bgmClip;
            }
            
            Debug.Log($"[AudioManager] 배경음악 재생: {bgmClip.name}");
        }
        
        /// <summary>
        /// 씬별 BGM 재생
        /// </summary>
        public void PlaySceneBGM(string sceneName)
        {
            if (sceneBGMMap.ContainsKey(sceneName))
            {
                PlayBGM(sceneBGMMap[sceneName], true);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] {sceneName}에 대한 BGM이 설정되지 않았습니다.");
            }
        }
        
        /// <summary>
        /// BGM 페이드 아웃 후 새 BGM으로 전환
        /// </summary>
        private IEnumerator FadeToNewBGM(AudioClip newBGM)
        {
            if (isFading) yield break;
            isFading = true;
            
            // 현재 BGM 페이드 아웃
            yield return StartCoroutine(FadeOut());
            
            // 새 BGM 재생
            bgmSource.clip = newBGM;
            bgmSource.volume = 0f;
            bgmSource.Play();
            currentBGM = newBGM;
            
            // 새 BGM 페이드 인
            yield return StartCoroutine(FadeIn());
            
            isFading = false;
        }
        
        /// <summary>
        /// BGM 페이드 아웃
        /// </summary>
        private IEnumerator FadeOut()
        {
            float startVolume = bgmSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeTime);
                yield return null;
            }
            
            bgmSource.volume = 0f;
        }
        
        /// <summary>
        /// BGM 페이드 인
        /// </summary>
        private IEnumerator FadeIn()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, bgmVolume, elapsedTime / fadeTime);
                yield return null;
            }
            
            bgmSource.volume = bgmVolume;
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
            PlayBGM(mainMenuBGM, true);
        }
        
        /// <summary>
        /// 전투 BGM 재생
        /// </summary>
        public void PlayBattleBGM()
        {
            PlayBGM(battleBGM, true);
        }
        
        /// <summary>
        /// 상점 BGM 재생
        /// </summary>
        public void PlayShopBGM()
        {
            PlayBGM(shopBGM, true);
        }
        
        /// <summary>
        /// 인벤토리 BGM 재생
        /// </summary>
        public void PlayInventoryBGM()
        {
            PlayBGM(inventoryBGM, true);
        }
        
        /// <summary>
        /// BGM 정지
        /// </summary>
        public void StopBGM()
        {
            if (bgmSource.isPlaying)
            {
                StartCoroutine(FadeOutAndStop());
            }
        }
        
        /// <summary>
        /// BGM 페이드 아웃 후 정지
        /// </summary>
        private IEnumerator FadeOutAndStop()
        {
            yield return StartCoroutine(FadeOut());
            bgmSource.Stop();
            currentBGM = null;
        }
        
        /// <summary>
        /// 현재 재생 중인 BGM 이름 반환
        /// </summary>
        public string GetCurrentBGMName()
        {
            return currentBGM != null ? currentBGM.name : "None";
        }
        
        /// <summary>
        /// 씬별 BGM 매핑 추가
        /// </summary>
        public void AddSceneBGM(string sceneName, AudioClip bgmClip)
        {
            if (sceneBGMMap == null)
            {
                sceneBGMMap = new Dictionary<string, AudioClip>();
            }
            
            sceneBGMMap[sceneName] = bgmClip;
            Debug.Log($"[AudioManager] {sceneName}에 {bgmClip.name} BGM 추가");
        }
        
        /// <summary>
        /// 현재 BGM 볼륨 반환
        /// </summary>
        public float BGMVolume => bgmVolume;
        
        /// <summary>
        /// 현재 SFX 볼륨 반환
        /// </summary>
        public float SFXVolume => sfxVolume;
        
        /// <summary>
        /// 오브젝트 파괴 시 이벤트 구독 해제
        /// </summary>
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
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
