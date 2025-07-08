using UnityEngine;

namespace AnimationSystem.Manager
{
    /// <summary>
    /// 애니메이션 시스템의 자동 초기화를 담당하는 클래스
    /// 게임 시작 시 애니메이션 시스템이 올바르게 설정되도록 합니다.
    /// </summary>
    public class AnimationSystemInitializer : MonoBehaviour
    {
        [Header("초기화 설정")]
        [SerializeField] private bool autoInitialize = true;

        private void Awake()
        {
            if (autoInitialize)
            {
                InitializeAnimationSystem();
            }
        }

        /// <summary>
        /// 애니메이션 시스템을 초기화합니다.
        /// </summary>
        public void InitializeAnimationSystem()
        {
            Debug.Log("[AnimationSystemInitializer] 애니메이션 시스템 초기화 시작");

            // 애니메이션 큐 매니저 초기화
            // AnimationQueueManager.Instance ... // TODO: 큐 시스템 제거됨, 관련 코드 삭제

            Debug.Log("[AnimationSystemInitializer] 애니메이션 시스템 초기화 완료");
        }

        /// <summary>
        /// 수동으로 초기화를 실행합니다.
        /// </summary>
        [ContextMenu("수동 초기화")]
        public void ManualInitialize()
        {
            InitializeAnimationSystem();
        }
    }
} 