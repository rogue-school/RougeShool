using UnityEngine;
using Game.SkillCardSystem.Interface;

namespace AnimationSystem.Interface
{
    /// <summary>
    /// 스킬카드 애니메이션 전용 파사드 인터페이스
    /// 단일 책임 원칙에 따라 스킬카드 애니메이션만 담당
    /// </summary>
    public interface ISkillCardAnimationFacade
    {
        void PlaySkillCardAnimation(string cardId, string animationType, GameObject target);
        void PlaySkillCardAnimation(string cardId, string animationType, GameObject target, System.Action onComplete);
        void PlaySkillCardAnimation(ISkillCard card, string animationType, GameObject target, System.Action onComplete = null);
        void PlaySkillCardDragStartAnimation(ISkillCard card, GameObject target, System.Action onComplete = null);
        void PlaySkillCardDragEndAnimation(ISkillCard card, GameObject target, System.Action onComplete = null);
        void PlaySkillCardDropAnimation(ISkillCard card, GameObject target, System.Action onComplete = null);
        void VanishCharacterSkillCards(string characterName, bool isPlayerCharacter, System.Action onComplete = null);
    }
} 