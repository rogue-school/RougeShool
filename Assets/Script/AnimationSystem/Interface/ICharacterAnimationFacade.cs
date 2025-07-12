using UnityEngine;

namespace AnimationSystem.Interface
{
    /// <summary>
    /// 캐릭터 애니메이션 전용 파사드 인터페이스
    /// 단일 책임 원칙에 따라 캐릭터 애니메이션만 담당
    /// </summary>
    public interface ICharacterAnimationFacade
    {
        void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null);
        void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null);
        void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false);
        void PlayPlayerCharacterDeathAnimation(string characterId, GameObject target);
        void PlayEnemyCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null);
        void PlayCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null, bool isEnemy = false);
    }
} 