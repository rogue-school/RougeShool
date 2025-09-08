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
        
        [Header("오디오 풀링")]
        [SerializeField] private AudioPoolManager audioPoolManager;  // 오디오 풀 매니저
        
        [Header("오디오 설정")]
        [SerializeField] private float bgmVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float fadeTime = 1.0f;  // 페이드 시간
        
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
            
            // 오디오 풀 매니저 초기화
            if (audioPoolManager == null)
            {
                audioPoolManager = gameObject.AddComponent<AudioPoolManager>();
            }
            
            Debug.Log("[AudioManager] 오디오 시스템 초기화 완료 (풀링 포함)");
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
        /// 효과음 재생 (기존 방식 - 단순한 SFX용)
        /// </summary>
        public void PlaySFX(AudioClip sfxClip)
        {
            if (sfxClip == null) return;
            
            sfxSource.PlayOneShot(sfxClip, sfxVolume);
            
            Debug.Log($"[AudioManager] 효과음 재생: {sfxClip.name}");
        }

        /// <summary>
        /// 효과음 재생 (풀링 방식 - 전투/UI 사운드용)
        /// </summary>
        public void PlaySFXWithPool(AudioClip sfxClip, float volume = 1.0f)
        {
            if (sfxClip == null || audioPoolManager == null) return;
            
            audioPoolManager.PlaySound(sfxClip, volume);
            
            Debug.Log($"[AudioManager] 풀링 효과음 재생: {sfxClip.name}");
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
        /// 현재 BGM 볼륨 반환
        /// </summary>
        public float BGMVolume => bgmVolume;
        
        /// <summary>
        /// 현재 SFX 볼륨 반환
        /// </summary>
        public float SFXVolume => sfxVolume;

        #region 전투 사운드

        /// <summary>
        /// 카드 사용 사운드 재생
        /// </summary>
        public void PlayCardUseSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/card");
            PlaySFXWithPool(clip, 0.8f);
        }

        /// <summary>
        /// 적 처치 사운드 재생
        /// </summary>
        public void PlayEnemyDefeatSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/magic_01");
            PlaySFXWithPool(clip, 1.0f);
        }

        /// <summary>
        /// 스킬 발동 사운드 재생
        /// </summary>
        public void PlaySkillActivationSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/laser_01");
            PlaySFXWithPool(clip, 0.9f);
        }

        /// <summary>
        /// 턴 시작 사운드 재생
        /// </summary>
        public void PlayTurnStartSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/electronic_01");
            PlaySFXWithPool(clip, 0.7f);
        }

        /// <summary>
        /// 턴 완료 사운드 재생
        /// </summary>
        public void PlayTurnCompleteSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/electronic_02");
            PlaySFXWithPool(clip, 0.7f);
        }

        /// <summary>
        /// 준보스 처치 사운드 재생
        /// </summary>
        public void PlaySubBossDefeatSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/magic_02");
            PlaySFXWithPool(clip, 1.0f);
        }

        /// <summary>
        /// 보스 처치 사운드 재생
        /// </summary>
        public void PlayBossDefeatSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/magic_03");
            PlaySFXWithPool(clip, 1.0f);
        }

        #endregion

        #region UI 사운드

        /// <summary>
        /// 버튼 클릭 사운드 재생
        /// </summary>
        public void PlayButtonClickSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/electronic_01");
            PlaySFXWithPool(clip, 0.5f);
        }

        /// <summary>
        /// 카드 드래그 사운드 재생
        /// </summary>
        public void PlayCardDragSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/card");
            PlaySFXWithPool(clip, 0.6f);
        }

        /// <summary>
        /// 카드 드롭 사운드 재생
        /// </summary>
        public void PlayCardDropSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/card");
            PlaySFXWithPool(clip, 0.8f);
        }

        /// <summary>
        /// 메뉴 열기 사운드 재생
        /// </summary>
        public void PlayMenuOpenSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/electronic_01");
            PlaySFXWithPool(clip, 0.6f);
        }

        /// <summary>
        /// 메뉴 닫기 사운드 재생
        /// </summary>
        public void PlayMenuCloseSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/electronic_02");
            PlaySFXWithPool(clip, 0.6f);
        }

        /// <summary>
        /// 힐 사운드 재생
        /// </summary>
        public void PlayHealSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/heal");
            PlaySFXWithPool(clip, 0.8f);
        }

        /// <summary>
        /// 방패 사운드 재생
        /// </summary>
        public void PlayShieldSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/방패");
            PlaySFXWithPool(clip, 0.7f);
        }

        /// <summary>
        /// 셔플 사운드 재생
        /// </summary>
        public void PlayShuffleSound()
        {
            AudioClip clip = Resources.Load<AudioClip>("Sounds/ShootingSound/셔플");
            PlaySFXWithPool(clip, 0.8f);
        }

        #endregion

        #region 오디오 풀 관리

        /// <summary>
        /// 오디오 풀 매니저 가져오기
        /// </summary>
        public AudioPoolManager GetAudioPoolManager()
        {
            return audioPoolManager;
        }

        /// <summary>
        /// 오디오 풀 상태 정보 출력
        /// </summary>
        [ContextMenu("오디오 풀 상태 출력")]
        public void PrintAudioPoolStatus()
        {
            if (audioPoolManager != null)
            {
                audioPoolManager.PrintPoolStatus();
            }
            else
            {
                Debug.LogWarning("[AudioManager] 오디오 풀 매니저가 없습니다.");
            }
        }

        /// <summary>
        /// 오디오 풀 쿨다운 상태 출력
        /// </summary>
        [ContextMenu("오디오 풀 쿨다운 상태 출력")]
        public void PrintAudioPoolCooldownStatus()
        {
            if (audioPoolManager != null)
            {
                audioPoolManager.PrintCooldownStatus();
            }
            else
            {
                Debug.LogWarning("[AudioManager] 오디오 풀 매니저가 없습니다.");
            }
        }

        #endregion
        
        // (주의) 씬별 BGM 자동 전환 로직은 제거되었습니다. 
        // 전역 이벤트/데이터 기반으로 PlayBGM(AudioClip) 또는 상위 서비스에서 호출하세요.
        
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
