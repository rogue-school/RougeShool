using UnityEngine;

namespace Game.AnimationSystem.Manager
{
    /// <summary>
    /// 애니메이션 시스템의 자동 초기화를 담당하는 클래스
    /// 게임 시작 시 애니메이션 시스템이 올바르게 설정되도록 합니다.
    /// </summary>
    public class AnimationSystemInitializer : MonoBehaviour
    {
        private static bool initialized = false;

        private void Awake()
        {
            if (initialized)
                return;

            initialized = true;
            // 시스템 초기화 성공/실패만 로그로 남김 (필요시)
            // Debug.Log("[AnimationSystemInitializer] 애니메이션 시스템 초기화 시작");
            // Debug.Log("[AnimationSystemInitializer] 애니메이션 시스템 초기화 완료");
        }
    }
} 