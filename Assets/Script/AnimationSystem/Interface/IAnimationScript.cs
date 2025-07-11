using UnityEngine;

namespace AnimationSystem.Interface
{
    /// <summary>
    /// 모든 애니메이션 스크립트의 기본 인터페이스
    /// </summary>
    public interface IAnimationScript
    {
        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="animationType">애니메이션 타입</param>
        /// <param name="onComplete">완료 콜백</param>
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