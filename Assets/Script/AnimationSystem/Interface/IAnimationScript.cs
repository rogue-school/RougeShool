using UnityEngine;

namespace AnimationSystem.Interface
{
    /// <summary>
    /// 애니메이션 스크립트들이 구현해야 하는 인터페이스
    /// </summary>
    public interface IAnimationScript
    {
        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="target">애니메이션을 적용할 타겟 오브젝트</param>
        /// <param name="animationType">애니메이션 타입</param>
        void PlayAnimation(GameObject target, string animationType);
    }
} 