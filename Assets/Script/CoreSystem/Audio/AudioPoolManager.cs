using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.CoreSystem.Utility;

namespace Game.CoreSystem.Audio
{
    /// <summary>
    /// 오디오 풀 매니저
    /// 사운드 중복 방지 + AudioSource 풀링으로 성능 최적화
    /// </summary>
    public class AudioPoolManager : MonoBehaviour
    {
        #region 인스펙터 필드

        [Header("오디오 풀 설정")]
        [SerializeField] private int poolSize = 10;
        [SerializeField] private float soundCooldown = 0.1f;
        [SerializeField] private bool enableCooldown = true;
        [SerializeField] private bool enablePriority = true;

        #endregion

        #region 내부 상태

        private Queue<AudioSource> audioSourcePool = new();
        private Dictionary<string, float> lastPlayTime = new();
        private Dictionary<string, AudioSource> playingSounds = new();
        private Dictionary<string, int> soundPriority = new();
        private bool isInitialized = false;

        #endregion

        #region 초기화

        private void Awake()
        {
            InitializeAudioPool();
        }

        /// <summary>
        /// 오디오 풀 초기화
        /// </summary>
        private void InitializeAudioPool()
        {
            // AudioSource 풀 생성
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSourcePool.Enqueue(audioSource);
            }

            // 사운드 우선순위 설정
            InitializeSoundPriority();

            isInitialized = true;
            GameLogger.LogDebug($"[AudioPoolManager] 오디오 풀 초기화 완료: {poolSize}개", GameLogger.LogCategory.Audio);
        }

        /// <summary>
        /// 사운드 우선순위 초기화
        /// </summary>
        private void InitializeSoundPriority()
        {
            soundPriority = new Dictionary<string, int>
            {
                // 전투 사운드 (높은 우선순위)
                { "enemy_defeat", 10 },
                { "skill_activation", 9 },
                { "card_use", 8 },
                { "turn_start", 7 },
                { "turn_complete", 6 },
                
                // UI 사운드 (중간 우선순위)
                { "button_click", 5 },
                { "card_drag", 4 },
                { "card_drop", 4 },
                { "menu_open", 3 },
                { "menu_close", 3 },
                
                // 기타 사운드 (낮은 우선순위)
                { "default", 1 }
            };
        }

        #endregion

        #region 사운드 재생

        /// <summary>
        /// 사운드 재생 (중복 방지 + 풀링)
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="volume">볼륨</param>
        /// <param name="priority">우선순위 (높을수록 우선)</param>
        public void PlaySound(AudioClip clip, float volume = 1.0f, int priority = 1)
        {
            if (clip == null || !isInitialized)
            {
                GameLogger.LogDebug("[AudioPoolManager] 사운드 재생 실패: 클립이 null이거나 초기화되지 않음", GameLogger.LogCategory.Audio);
                return;
            }

            string clipName = clip.name;

            // 쿨다운 체크
            if (enableCooldown && IsInCooldown(clipName))
            {
                GameLogger.LogDebug($"[AudioPoolManager] 사운드 재생 방지: {clipName} (쿨다운 중)", GameLogger.LogCategory.Audio);
                return;
            }

            // 우선순위 체크
            if (enablePriority && !CanPlayWithPriority(clipName, priority))
            {
                GameLogger.LogDebug($"[AudioPoolManager] 사운드 재생 방지: {clipName} (우선순위 낮음)", GameLogger.LogCategory.Audio);
                return;
            }

            // AudioSource 풀에서 가져오기
            AudioSource audioSource = GetAudioSourceFromPool();
            if (audioSource == null)
            {
                GameLogger.LogDebug("[AudioPoolManager] AudioSource 풀에서 가져올 수 없음", GameLogger.LogCategory.Audio);
                return;
            }

            // 사운드 재생
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();

            // 재생 시간 기록
            lastPlayTime[clipName] = Time.time;
            playingSounds[clipName] = audioSource;

            // 재생 완료 후 풀에 반환
            StartCoroutine(ReturnToPoolAfterPlay(audioSource, clipName));

            GameLogger.LogDebug($"[AudioPoolManager] 사운드 재생: {clipName} (볼륨: {volume}, 우선순위: {priority})", GameLogger.LogCategory.Audio);
        }

        /// <summary>
        /// 사운드 재생 (우선순위 자동 설정)
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="volume">볼륨</param>
        public void PlaySound(AudioClip clip, float volume = 1.0f)
        {
            int priority = GetSoundPriority(clip.name);
            PlaySound(clip, volume, priority);
        }

        #endregion

        #region 내부 메서드

        /// <summary>
        /// 쿨다운 중인지 확인
        /// </summary>
        private bool IsInCooldown(string clipName)
        {
            if (!lastPlayTime.ContainsKey(clipName))
                return false;

            float timeSinceLastPlay = Time.time - lastPlayTime[clipName];
            return timeSinceLastPlay < soundCooldown;
        }

        /// <summary>
        /// 우선순위에 따라 재생 가능한지 확인
        /// </summary>
        private bool CanPlayWithPriority(string clipName, int priority)
        {
            // 현재 재생 중인 사운드 중에 더 높은 우선순위가 있는지 확인
            foreach (var kvp in playingSounds)
            {
                string playingClipName = kvp.Key;
                int playingPriority = GetSoundPriority(playingClipName);
                
                if (playingPriority > priority)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 사운드 우선순위 가져오기
        /// </summary>
        private int GetSoundPriority(string clipName)
        {
            if (soundPriority.ContainsKey(clipName))
                return soundPriority[clipName];
            
            return soundPriority["default"];
        }

        /// <summary>
        /// AudioSource 풀에서 가져오기
        /// </summary>
        private AudioSource GetAudioSourceFromPool()
        {
            if (audioSourcePool.Count > 0)
            {
                return audioSourcePool.Dequeue();
            }

            // 풀이 비어있으면 새로 생성
            AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
            newAudioSource.playOnAwake = false;
            newAudioSource.loop = false;
            
            GameLogger.LogDebug("[AudioPoolManager] AudioSource 풀이 비어있어 새로 생성", GameLogger.LogCategory.Audio);
            return newAudioSource;
        }

        /// <summary>
        /// 사운드 재생 완료 후 풀에 반환
        /// </summary>
        private IEnumerator ReturnToPoolAfterPlay(AudioSource audioSource, string clipName)
        {
            // 사운드 길이만큼 대기
            yield return new WaitForSeconds(audioSource.clip.length);

            // 풀에 반환
            audioSourcePool.Enqueue(audioSource);
            playingSounds.Remove(clipName);

            GameLogger.LogDebug($"[AudioPoolManager] 사운드 재생 완료: {clipName}", GameLogger.LogCategory.Audio);
        }

        #endregion

        #region 설정 관리

        /// <summary>
        /// 쿨다운 시간 설정
        /// </summary>
        /// <param name="cooldown">쿨다운 시간 (초)</param>
        public void SetSoundCooldown(float cooldown)
        {
            soundCooldown = Mathf.Max(0f, cooldown);
            GameLogger.LogDebug($"[AudioPoolManager] 쿨다운 시간 설정: {soundCooldown}초", GameLogger.LogCategory.Audio);
        }

        /// <summary>
        /// 쿨다운 활성화/비활성화
        /// </summary>
        /// <param name="enabled">활성화 여부</param>
        public void SetCooldownEnabled(bool enabled)
        {
            enableCooldown = enabled;
            GameLogger.LogDebug($"[AudioPoolManager] 쿨다운 {(enabled ? "활성화" : "비활성화")}", GameLogger.LogCategory.Audio);
        }

        /// <summary>
        /// 우선순위 활성화/비활성화
        /// </summary>
        /// <param name="enabled">활성화 여부</param>
        public void SetPriorityEnabled(bool enabled)
        {
            enablePriority = enabled;
            GameLogger.LogDebug($"[AudioPoolManager] 우선순위 {(enabled ? "활성화" : "비활성화")}", GameLogger.LogCategory.Audio);
        }

        /// <summary>
        /// 풀 크기 설정
        /// </summary>
        /// <param name="size">풀 크기</param>
        public void SetPoolSize(int size)
        {
            poolSize = Mathf.Max(1, size);
            GameLogger.LogDebug($"[AudioPoolManager] 풀 크기 설정: {poolSize}", GameLogger.LogCategory.Audio);
        }

        #endregion

        #region 상태 정보

        /// <summary>
        /// 현재 풀 상태 정보
        /// </summary>
        [ContextMenu("풀 상태 정보 출력")]
        public void PrintPoolStatus()
        {
            Debug.Log($"[AudioPoolManager] 풀 상태:");
            Debug.Log($"  - 사용 가능한 AudioSource: {audioSourcePool.Count}개");
            Debug.Log($"  - 재생 중인 사운드: {playingSounds.Count}개");
            Debug.Log($"  - 쿨다운 활성화: {enableCooldown}");
            Debug.Log($"  - 우선순위 활성화: {enablePriority}");
            Debug.Log($"  - 쿨다운 시간: {soundCooldown}초");

            if (playingSounds.Count > 0)
            {
                Debug.Log("  - 재생 중인 사운드 목록:");
                foreach (var kvp in playingSounds)
                {
                    Debug.Log($"    * {kvp.Key}");
                }
            }
        }

        /// <summary>
        /// 현재 쿨다운 상태 정보
        /// </summary>
        [ContextMenu("쿨다운 상태 정보 출력")]
        public void PrintCooldownStatus()
        {
            Debug.Log($"[AudioPoolManager] 쿨다운 상태:");
            
            if (lastPlayTime.Count == 0)
            {
                Debug.Log("  - 쿨다운 중인 사운드 없음");
                return;
            }

            foreach (var kvp in lastPlayTime)
            {
                string clipName = kvp.Key;
                float lastPlay = kvp.Value;
                float timeSinceLastPlay = Time.time - lastPlay;
                bool isInCooldown = timeSinceLastPlay < soundCooldown;
                
                Debug.Log($"  - {clipName}: {(isInCooldown ? "쿨다운 중" : "재생 가능")} ({timeSinceLastPlay:F2}초 전)");
            }
        }

        #endregion
    }
}
