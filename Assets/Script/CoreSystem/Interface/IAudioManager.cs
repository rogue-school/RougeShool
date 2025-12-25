using UnityEngine;
using Game.CharacterSystem.Data;

namespace Game.CoreSystem.Interface
{
    /// <summary>
    /// 오디오 관리 인터페이스
    /// </summary>
    public interface IAudioManager
    {
        /// <summary>
        /// BGM 볼륨 (읽기 전용)
        /// </summary>
        float BgmVolume { get; }
        
        /// <summary>
        /// SFX 볼륨 (읽기 전용)
        /// </summary>
        float SfxVolume { get; }
        
        /// <summary>
        /// BGM 재생
        /// </summary>
        /// <param name="clip">재생할 BGM 클립</param>
        /// <param name="fadeIn">페이드 인 효과 사용 여부 (기본값: false)</param>
        void PlayBGM(AudioClip clip, bool fadeIn = false);
        
        /// <summary>
        /// BGM 정지
        /// </summary>
        void StopBGM();
        
        /// <summary>
        /// 효과음 재생
        /// </summary>
        /// <param name="clip">재생할 SFX 클립</param>
        void PlaySFX(AudioClip clip);
        
        /// <summary>
        /// 효과음 재생 (풀링 사용)
        /// </summary>
        /// <param name="clip">재생할 SFX 클립</param>
        /// <param name="volume">볼륨 (기본값: 1.0)</param>
        /// <param name="priority">우선순위 (기본값: 5)</param>
        void PlaySFXWithPool(AudioClip clip, float volume = 1.0f, int priority = 5);
        
        /// <summary>
        /// BGM 볼륨 설정
        /// </summary>
        /// <param name="volume">볼륨 값 (0.0 ~ 1.0)</param>
        void SetBGMVolume(float volume);
        
        /// <summary>
        /// SFX 볼륨 설정
        /// </summary>
        /// <param name="volume">볼륨 값 (0.0 ~ 1.0)</param>
        void SetSFXVolume(float volume);
        
        /// <summary>
        /// 마스터 볼륨 설정
        /// </summary>
        /// <param name="volume">볼륨 값 (0.0 ~ 1.0)</param>
        void SetMasterVolume(float volume);
        
        /// <summary>
        /// 적별 BGM 재생
        /// </summary>
        /// <param name="enemyData">적 캐릭터 데이터</param>
        void PlayEnemyBGM(EnemyCharacterData enemyData);
    }
}

