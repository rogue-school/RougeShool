using UnityEngine;
using AnimationSystem.Interface;
using AnimationSystem.Animator.SkillCardAnimation.VanishAnimation;

namespace AnimationSystem.Service
{
    /// <summary>
    /// 스킬카드 소멸 애니메이션 팩토리 구현체
    /// 구체적인 구현체 생성 로직을 담당
    /// </summary>
    public class SkillCardVanishAnimationFactory : MonoBehaviour, ISkillCardVanishAnimationFactory
    {
        private const string COMPONENT_NAME = "SkillCardVanishAnimationFactory";

        public ISkillCardVanishAnimationScript CreateVanishAnimation(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[{COMPONENT_NAME}] target이 null입니다.");
                return null;
            }
            
            var existingVanishAnim = target.GetComponent<DefaultSkillCardVanishAnimation>();
            if (existingVanishAnim != null)
            {
                return (ISkillCardVanishAnimationScript)existingVanishAnim;
            }
            
            var vanishAnim = target.AddComponent<DefaultSkillCardVanishAnimation>();
            Debug.Log($"[{COMPONENT_NAME}] 소멸 애니메이션 생성: {target.name}");
            
            return (ISkillCardVanishAnimationScript)vanishAnim;
        }
        
        public ISkillCardVanishAnimationScript AddVanishAnimationToTarget(GameObject target)
            => CreateVanishAnimation(target);
    }
} 