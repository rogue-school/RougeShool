using UnityEngine;
using System.Collections.Generic;
using AnimationSystem.Interface;
using AnimationSystem.Service;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

namespace AnimationSystem.Manager
{
    /// <summary>
    /// 스킬카드 애니메이션 전용 파사드 구현체
    /// 단일 책임 원칙에 따라 스킬카드 애니메이션만 담당
    /// </summary>
    public class SkillCardAnimationFacade : MonoBehaviour, ISkillCardAnimationFacade
    {
        [Header("의존성 주입")]
        [SerializeField] private ISkillCardFinder skillCardFinder;
        [SerializeField] private ISkillCardVanishAnimationFactory vanishAnimationFactory;
        
        private const string VANISH_ANIMATION_TYPE = "vanish";
        private const string COMPONENT_NAME = "SkillCardAnimationFacade";
        private const int INITIAL_COUNT = 0;
        
        private void Awake()
        {
            skillCardFinder ??= gameObject.AddComponent<SkillCardFinder>();
            vanishAnimationFactory ??= gameObject.AddComponent<SkillCardVanishAnimationFactory>();
        }
        
        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target)
            => AnimationDatabaseManager.Instance.PlayPlayerSkillCardAnimation(cardId, target, animationType);
        
        public void PlaySkillCardAnimation(string cardId, string animationType, GameObject target, System.Action onComplete)
            => AnimationDatabaseManager.Instance.PlayPlayerSkillCardAnimation(cardId, target, animationType, onComplete);
        
        public void PlaySkillCardAnimation(ISkillCard card, string animationType, GameObject target, System.Action onComplete = null)
        {
            if (!ValidateCard(card, onComplete)) return;
            
            var owner = card.GetOwner();
            var animationManager = AnimationDatabaseManager.Instance;
            
            if (owner == SlotOwner.ENEMY)
                animationManager.PlayEnemySkillCardAnimation(card.CardData.Name, target, animationType, onComplete);
            else
                animationManager.PlayPlayerSkillCardAnimation(card.CardData.Name, target, animationType, onComplete);
        }
        
        public void PlaySkillCardDragStartAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (!ValidateCard(card, onComplete)) return;
            AnimationDatabaseManager.Instance.PlaySkillCardDragStartAnimation(card, target, onComplete);
        }
        
        public void PlaySkillCardDragEndAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (!ValidateCard(card, onComplete)) return;
            AnimationDatabaseManager.Instance.PlaySkillCardDragEndAnimation(card, target, onComplete);
        }
        
        public void PlaySkillCardDropAnimation(ISkillCard card, GameObject target, System.Action onComplete = null)
        {
            if (!ValidateCard(card, onComplete)) return;
            AnimationDatabaseManager.Instance.PlaySkillCardDropAnimation(card, target, onComplete);
        }
        
        public void VanishCharacterSkillCards(string characterName, bool isPlayerCharacter, System.Action onComplete = null)
        {
            Debug.Log($"[{COMPONENT_NAME}] 캐릭터 사망으로 인한 스킬카드 소멸 시작: {characterName}, 플레이어: {isPlayerCharacter}");
            
            var skillCards = skillCardFinder.FindCharacterSkillCards(characterName, isPlayerCharacter);
            
            if (skillCards.Count == 0)
            {
                Debug.Log($"[{COMPONENT_NAME}] 소멸할 스킬카드가 없습니다: {characterName}");
                onComplete?.Invoke();
                return;
            }
            
            Debug.Log($"[{COMPONENT_NAME}] 소멸할 스킬카드 수: {skillCards.Count}");
            
            var animationTracker = new AnimationTracker(skillCards.Count, characterName, onComplete);
            
            foreach (var skillCard in skillCards)
            {
                if (skillCard == null) continue;
                
                var vanishAnim = vanishAnimationFactory.AddVanishAnimationToTarget(skillCard);
                if (vanishAnim == null)
                {
                    Debug.LogWarning($"[{COMPONENT_NAME}] 소멸 애니메이션 생성 실패: {skillCard.name}");
                    animationTracker.IncrementCompleted();
                    continue;
                }
                
                vanishAnim.PlayAnimation(VANISH_ANIMATION_TYPE, () => animationTracker.OnAnimationComplete());
            }
        }
        
        private bool ValidateCard(ISkillCard card, System.Action onComplete)
        {
            if (card != null) return true;
            
            Debug.LogWarning($"[{COMPONENT_NAME}] card가 null입니다.");
            onComplete?.Invoke();
            return false;
        }
        
        /// <summary>
        /// 애니메이션 완료 추적을 위한 헬퍼 클래스
        /// </summary>
        private class AnimationTracker
        {
            private readonly int totalCount;
            private readonly string characterName;
            private readonly System.Action onComplete;
            private int completedCount;
            
            public AnimationTracker(int totalCount, string characterName, System.Action onComplete)
            {
                this.totalCount = totalCount;
                this.characterName = characterName;
                this.onComplete = onComplete;
                this.completedCount = INITIAL_COUNT;
            }
            
            public void IncrementCompleted() => completedCount++;
            
            public void OnAnimationComplete()
            {
                completedCount++;
                Debug.Log($"[{COMPONENT_NAME}] 스킬카드 소멸 완료: {completedCount}/{totalCount}");
                
                if (completedCount >= totalCount)
                {
                    Debug.Log($"[{COMPONENT_NAME}] 모든 스킬카드 소멸 완료: {characterName}");
                    onComplete?.Invoke();
                }
            }
        }
    }
} 