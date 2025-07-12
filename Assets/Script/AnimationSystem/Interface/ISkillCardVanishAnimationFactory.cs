using UnityEngine;

namespace AnimationSystem.Interface
{
    /// <summary>
    /// 스킬카드 소멸 애니메이션 팩토리 인터페이스
    /// 구체적인 구현체 생성 로직을 분리하기 위한 팩토리 패턴
    /// </summary>
    public interface ISkillCardVanishAnimationFactory
    {
        /// <summary>
        /// 스킬카드 소멸 애니메이션을 생성합니다.
        /// </summary>
        /// <param name="target">애니메이션을 적용할 GameObject</param>
        /// <returns>소멸 애니메이션 스크립트</returns>
        ISkillCardVanishAnimationScript CreateVanishAnimation(GameObject target);
        
        /// <summary>
        /// 스킬카드에 소멸 애니메이션을 추가합니다.
        /// </summary>
        /// <param name="target">애니메이션을 적용할 GameObject</param>
        /// <returns>추가된 소멸 애니메이션 스크립트</returns>
        ISkillCardVanishAnimationScript AddVanishAnimationToTarget(GameObject target);
    }
} 