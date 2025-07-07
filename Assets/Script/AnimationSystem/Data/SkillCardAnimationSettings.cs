using UnityEngine;

namespace AnimationSystem.Data
{
    /// <summary>
    /// 스킬 카드 애니메이션 설정을 위한 구조체
    /// </summary>
    [System.Serializable]
    public struct SkillCardAnimationSettings
    {
        [Header("애니메이션 스크립트 타입")]
        [SerializeField] private string animationScriptType;
        
        [Header("애니메이션 파라미터")]
        [SerializeField] private float duration;
        [SerializeField] private bool useEasing;
        [SerializeField] private AnimationCurve customCurve;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float scale;
        
        /// <summary>
        /// 애니메이션 스크립트 타입
        /// </summary>
        public string AnimationScriptType
        {
            get => animationScriptType;
            set => animationScriptType = value;
        }
        
        /// <summary>
        /// 애니메이션 지속 시간
        /// </summary>
        public float Duration
        {
            get => duration;
            set => duration = value;
        }
        
        /// <summary>
        /// 이징 사용 여부
        /// </summary>
        public bool UseEasing
        {
            get => useEasing;
            set => useEasing = value;
        }
        
        /// <summary>
        /// 커스텀 애니메이션 커브
        /// </summary>
        public AnimationCurve CustomCurve
        {
            get => customCurve;
            set => customCurve = value;
        }
        
        /// <summary>
        /// 위치 오프셋
        /// </summary>
        public Vector3 Offset
        {
            get => offset;
            set => offset = value;
        }
        
        /// <summary>
        /// 스케일 값
        /// </summary>
        public float Scale
        {
            get => scale;
            set => scale = value;
        }
        
        /// <summary>
        /// 기본값을 반환하는 정적 메서드
        /// </summary>
        public static SkillCardAnimationSettings Default => new SkillCardAnimationSettings(string.Empty, 1.0f, true, Vector3.zero, 1.0f);
        
        /// <summary>
        /// 설정이 비어있는지 확인합니다.
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(animationScriptType);
        }
        
        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="target">타겟 오브젝트</param>
        /// <param name="animationType">애니메이션 타입</param>
        public void PlayAnimation(GameObject target, string animationType)
        {
            if (IsEmpty() || target == null)
                return;

            // animationScriptType이 예: "AnimationSystem.Animator.SkillCardSpawnAnimator"
            var type = System.Type.GetType(animationScriptType);
            if (type == null)
            {
                Debug.LogError($"[SkillCardAnimationSettings] 애니메이션 타입을 찾을 수 없습니다: {animationScriptType}");
                return;
            }

            var animScript = target.GetComponent(type) as AnimationSystem.Interface.IAnimationScript;
            if (animScript == null)
            {
                animScript = target.AddComponent(type) as AnimationSystem.Interface.IAnimationScript;
            }
            if (animScript != null)
            {
                animScript.PlayAnimation(target, animationType);
            }
            else
            {
                Debug.LogError($"[SkillCardAnimationSettings] 애니메이션 스크립트 인스턴스화 실패: {animationScriptType}");
            }
        }
        
        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        /// <param name="scriptType">애니메이션 스크립트 타입</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="useEasing">이징 사용 여부</param>
        /// <param name="offset">위치 오프셋</param>
        /// <param name="scale">스케일 값</param>
        public SkillCardAnimationSettings(string scriptType, float duration = 1.0f, bool useEasing = true, Vector3 offset = default, float scale = 1.0f)
        {
            this.animationScriptType = scriptType;
            this.duration = duration;
            this.useEasing = useEasing;
            this.customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            this.offset = offset;
            this.scale = scale;
        }
    }
} 