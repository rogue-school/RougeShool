using UnityEngine;
using AnimationSystem.Interface;
using AnimationSystem.Data;

namespace AnimationSystem.Manager
{
    /// <summary>
    /// 캐릭터 애니메이션 전용 파사드 구현체
    /// 단일 책임 원칙에 따라 캐릭터 애니메이션만 담당
    /// </summary>
    public class CharacterAnimationFacade : MonoBehaviour, ICharacterAnimationFacade
    {
        private const string DEATH_ANIMATION_TYPE = "death";
        private const string COMPONENT_NAME = "CharacterAnimationFacade";

        public void PlayPlayerCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
            => AnimationDatabaseManager.Instance.PlayPlayerCharacterAnimation(characterId, target, animationType, onComplete);
        
        public void PlayEnemyCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null)
            => AnimationDatabaseManager.Instance.PlayEnemyCharacterAnimation(characterId, target, animationType, onComplete);
        
        public void PlayCharacterAnimation(string characterId, string animationType, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            if (isEnemy)
                PlayEnemyCharacterAnimation(characterId, animationType, target, onComplete);
            else
                PlayPlayerCharacterAnimation(characterId, animationType, target, onComplete);
        }
        
        public void PlayPlayerCharacterDeathAnimation(string characterId, GameObject target)
        {
            var entry = AnimationDatabaseManager.Instance.GetPlayerCharacterAnimationEntry(characterId);
            if (IsDeathAnimationValid(entry))
            {
                AnimationDatabaseManager.Instance.PlayPlayerCharacterAnimation(characterId, target, DEATH_ANIMATION_TYPE);
            }
        }
        
        public void PlayEnemyCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null)
        {
            var entry = AnimationDatabaseManager.Instance.GetEnemyCharacterAnimationEntry(characterId);
            if (IsDeathAnimationValid(entry))
            {
                AnimationDatabaseManager.Instance.PlayEnemyCharacterAnimation(characterId, target, DEATH_ANIMATION_TYPE, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }
        
        public void PlayCharacterDeathAnimation(string characterId, GameObject target, System.Action onComplete = null, bool isEnemy = false)
        {
            if (isEnemy)
                PlayEnemyCharacterDeathAnimation(characterId, target, onComplete);
            else
                PlayPlayerCharacterDeathAnimation(characterId, target);
        }

        private bool IsDeathAnimationValid(PlayerCharacterAnimationEntry entry)
        {
            if (entry?.DeathAnimation.IsEmpty() != false)
            {
                Debug.LogWarning($"[{COMPONENT_NAME}] 플레이어 캐릭터의 사망 애니메이션이 설정되지 않음");
                return false;
            }
            return true;
        }

        private bool IsDeathAnimationValid(EnemyCharacterAnimationEntry entry)
        {
            if (entry?.DeathAnimation.IsEmpty() != false)
            {
                Debug.LogWarning($"[{COMPONENT_NAME}] 적 캐릭터의 사망 애니메이션이 설정되지 않음");
                return false;
            }
            return true;
        }
    }
} 