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
        void PlayBGM(AudioClip clip, bool fadeIn = false);
        
        /// <summary>
        /// BGM 정지
        /// </summary>
        void StopBGM();
        
        /// <summary>
        /// 효과음 재생
        /// </summary>
        void PlaySFX(AudioClip clip);
        
        /// <summary>
        /// 효과음 재생 (풀링 사용)
        /// </summary>
        void PlaySFXWithPool(AudioClip clip, float volume = 1.0f, int priority = 5);
        
        /// <summary>
        /// BGM 볼륨 설정
        /// </summary>
        void SetBGMVolume(float volume);
        
        /// <summary>
        /// SFX 볼륨 설정
        /// </summary>
        void SetSFXVolume(float volume);
        
        /// <summary>
        /// 마스터 볼륨 설정
        /// </summary>
        void SetMasterVolume(float volume);
        
        /// <summary>
        /// 적별 BGM 재생
        /// </summary>
        void PlayEnemyBGM(EnemyCharacterData enemyData);
    }
}

