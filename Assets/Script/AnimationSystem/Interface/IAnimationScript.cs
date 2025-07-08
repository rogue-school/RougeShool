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
        /// <param name="animationType">애니메이션 타입</param>
        void PlayAnimation(string animationType, System.Action onComplete = null);
        
        /// <summary>
        /// 애니메이션을 중지합니다.
        /// </summary>
        void StopAnimation();
        
        /// <summary>
        /// 애니메이션을 즉시 완료합니다.
        /// </summary>
        void CompleteAnimation();
    }
} 