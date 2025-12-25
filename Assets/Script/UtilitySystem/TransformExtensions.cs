using UnityEngine;

namespace Game.UtilitySystem
{
    /// <summary>
    /// Transform에 대한 Extension 메서드
    /// 자식 Transform 찾기 로직을 통합하여 중복 코드를 제거합니다.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// 이름으로 자식 Transform을 찾습니다
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <param name="name">찾을 자식의 이름</param>
        /// <returns>찾은 Transform, 없으면 null</returns>
        public static Transform FindChildByName(this Transform parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                    return child;
            }

            return null;
        }

        /// <summary>
        /// 이름으로 자식 Transform을 재귀적으로 찾습니다
        /// </summary>
        /// <param name="parent">부모 Transform</param>
        /// <param name="name">찾을 자식의 이름</param>
        /// <returns>찾은 Transform, 없으면 null</returns>
        public static Transform FindChildByNameRecursive(this Transform parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                    return child;

                var found = child.FindChildByNameRecursive(name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}

