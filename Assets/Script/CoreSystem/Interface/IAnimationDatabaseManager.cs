using Game.AnimationSystem.Data;
using Game.SkillCardSystem.Interface;
using UnityEngine;
using System;

namespace Game.CoreSystem.Interface
{
    /// <summary>
    /// 애니메이션 데이터베이스 관리 인터페이스
    /// </summary>
    public interface IAnimationDatabaseManager
    {
        /// <summary>
        /// 통합 스킬카드 애니메이션 데이터베이스
        /// </summary>
        UnifiedSkillCardAnimationDatabase UnifiedSkillCardDatabase { get; }
        
        /// <summary>
        /// 플레이어 캐릭터 애니메이션 데이터베이스
        /// </summary>
        PlayerCharacterAnimationDatabase PlayerCharacterDatabase { get; }
        
        /// <summary>
        /// 적 캐릭터 애니메이션 데이터베이스
        /// </summary>
        EnemyCharacterAnimationDatabase EnemyCharacterDatabase { get; }
        
        /// <summary>
        /// 데이터베이스 로드
        /// </summary>
        void LoadDatabases();
        
        /// <summary>
        /// 캐시 초기화
        /// </summary>
        void InitializeCaching();
        
        /// <summary>
        /// 캐시 정리
        /// </summary>
        void ClearCache();
        
        /// <summary>
        /// 데이터베이스 재로드
        /// </summary>
        void ReloadDatabases();
        
        /// <summary>
        /// 플레이어 캐릭터 애니메이션 실행
        /// </summary>
        void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, Action onComplete = null);
        
        /// <summary>
        /// 적 캐릭터 애니메이션 실행
        /// </summary>
        void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, Action onComplete = null);
        
        /// <summary>
        /// 스킬카드 애니메이션 실행
        /// </summary>
        void PlaySkillCardAnimation(ISkillCard skillCard, GameObject target, string animationType, Action onComplete = null);
        
        /// <summary>
        /// 플레이어 캐릭터 애니메이션 엔트리 조회
        /// </summary>
        PlayerCharacterAnimationEntry GetPlayerCharacterAnimationEntry(string characterId);
        
        /// <summary>
        /// 적 캐릭터 애니메이션 엔트리 조회
        /// </summary>
        EnemyCharacterAnimationEntry GetEnemyCharacterAnimationEntry(string characterId);
        
        /// <summary>
        /// 데이터베이스 상태 디버그 출력
        /// </summary>
        void DebugDatabaseStatus();
    }
}
