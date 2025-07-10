using UnityEngine;

namespace AnimationSystem.Helper
{
    /// <summary>
    /// 애니메이션 관련 컴포넌트를 런타임에 안전하게 부착/획득하는 헬퍼 클래스
    /// </summary>
    public static class AnimationHelper
    {
        /// <summary>
        /// 지정한 GameObject에서 T 타입 컴포넌트를 가져오거나, 없으면 자동으로 AddComponent<T>() 후 반환합니다.
        /// </summary>
        public static T GetOrAddAnimator<T>(GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (comp == null)
                comp = obj.AddComponent<T>();
            return comp;
        }
    }
} 