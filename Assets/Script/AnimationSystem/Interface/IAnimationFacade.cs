using System;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Data;

namespace Game.AnimationSystem.Interface
{
    /// <summary>
    /// 애니메이션 시스템의 통합 인터페이스입니다.
    /// 모든 애니메이션 관련 기능을 중앙화된 방식으로 제공합니다.
    /// </summary>
    public interface IAnimationFacade
    {
        /// <summary>
        /// 스킬카드 애니메이션 실행 이벤트
        /// </summary>
        Action<ISkillCard, string> OnSkillCardAnimationPlayed { get; set; }
        
        /// <summary>
        /// 캐릭터 애니메이션 실행 이벤트
        /// </summary>
        Action<string, string, bool> OnCharacterAnimationPlayed { get; set; }

        /// <summary>
        /// 스킬카드 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="skillCard">애니메이션을 실행할 스킬카드</param>
        /// <param name="target">대상 GameObject</param>
        /// <param name="animationType">애니메이션 타입</param>
        /// <param name="onComplete">완료 콜백</param>
        void PlaySkillCardAnimation(ISkillCard skillCard, GameObject target, string animationType, Action onComplete = null);

        /// <summary>
        /// 스킬카드 드래그 시작 애니메이션을 실행합니다. (플레이어 전용)
        /// </summary>
        /// <param name="skillCard">애니메이션을 실행할 스킬카드</param>
        /// <param name="target">대상 GameObject</param>
        /// <param name="onComplete">완료 콜백</param>
        void PlaySkillCardDragStartAnimation(ISkillCard skillCard, GameObject target, Action onComplete = null);

        /// <summary>
        /// 스킬카드 드롭 애니메이션을 실행합니다. (플레이어 전용)
        /// </summary>
        /// <param name="skillCard">애니메이션을 실행할 스킬카드</param>
        /// <param name="target">대상 GameObject</param>
        /// <param name="onComplete">완료 콜백</param>
        void PlaySkillCardDropAnimation(ISkillCard skillCard, GameObject target, Action onComplete = null);

        /// <summary>
        /// 캐릭터 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="animationType">애니메이션 타입</param>
        /// <param name="target">대상 GameObject</param>
        /// <param name="onComplete">완료 콜백</param>
        /// <param name="isEnemy">적 캐릭터 여부</param>
        void PlayCharacterAnimation(string characterId, string animationType, GameObject target, Action onComplete = null, bool isEnemy = false);

        /// <summary>
        /// 캐릭터 사망 애니메이션을 실행합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="target">대상 GameObject</param>
        /// <param name="onComplete">완료 콜백</param>
        /// <param name="isEnemy">적 캐릭터 여부</param>
        void PlayCharacterDeathAnimation(string characterId, GameObject target, Action onComplete = null, bool isEnemy = false);

        /// <summary>
        /// 모든 애니메이션 데이터를 로드합니다.
        /// </summary>
        void LoadAllData();
        
        /// <summary>
        /// 핸드 소멸 애니메이션이 재생 중인지 확인
        /// </summary>
        bool IsHandVanishAnimationPlaying { get; }
        
        /// <summary>
        /// 캐릭터 사망 시 모든 핸드 카드 소멸 애니메이션 실행
        /// </summary>
        void VanishAllHandCardsOnCharacterDeath(string characterId, bool isEnemy = false);
        
        /// <summary>
        /// 적 캐릭터 애니메이션 실행
        /// </summary>
        void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, Action onComplete = null);
        
        /// <summary>
        /// 스킬카드 드래그 종료 애니메이션 실행
        /// </summary>
        void PlaySkillCardDragEndAnimation(ISkillCard skillCard, GameObject target, Action onComplete = null);
    }
}
