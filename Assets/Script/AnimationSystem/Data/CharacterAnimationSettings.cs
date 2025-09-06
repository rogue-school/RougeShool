using UnityEngine;
using DG.Tweening;

namespace Game.AnimationSystem.Data
{
    /// <summary>
    /// 캐릭터 애니메이션 설정을 위한 구조체
    /// </summary>
    [System.Serializable]
    public class CharacterAnimationSettings
    {
        [Header("애니메이션 스크립트 타입")]
        [SerializeField] private string animationScriptType;
        
        [Header("애니메이션 파라미터")]
        [SerializeField] private float duration;
        [SerializeField] private bool useEasing;
        [SerializeField] private AnimationCurve customCurve;
        
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
        /// 기본값을 반환하는 정적 메서드
        /// </summary>
        public static CharacterAnimationSettings Default => new CharacterAnimationSettings(string.Empty, 1.0f, true);
        
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
        public void PlayAnimation(GameObject target, string animationType, System.Action onComplete = null)
        {
            var scriptType = GetScriptTypeForAnimation(animationType);

            if (scriptType != null)
            {
                var script = target.GetComponent(scriptType) ?? target.AddComponent(scriptType);
                if (script == null)
                {
                    Debug.LogError($"[CharacterAnimationSettings] AddComponent 실패: {scriptType.FullName}");
                    onComplete?.Invoke();
                    return;
                }
                var animScript = script as AnimationSystem.Interface.IAnimationScript;
                if (animScript == null)
                {
                    Debug.LogError($"[CharacterAnimationSettings] IAnimationScript 캐스팅 실패: {scriptType.FullName}");
                    onComplete?.Invoke();
                    return;
                }
                animScript.PlayAnimation(animationType, onComplete);
            }
            else
            {
                Debug.LogError($"[CharacterAnimationSettings] 타입 조회 실패: {animationScriptType}");
                // Fallback: 기본 애니메이션(페이드아웃/페이드인 등) 실행
                PlayFallbackAnimation(target, animationType, onComplete);
            }
        }

        private void PlayFallbackAnimation(GameObject target, string animationType, System.Action onComplete)
        {
            var canvasGroup = target.GetComponent<UnityEngine.CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = target.AddComponent<UnityEngine.CanvasGroup>();

            if (animationType == "death")
            {
                canvasGroup.DOFade(0f, 0.5f).OnComplete(() => onComplete?.Invoke());
            }
            else if (animationType == "spawn")
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.5f).OnComplete(() => onComplete?.Invoke());
            }
            else
            {
                onComplete?.Invoke();
            }
        }
        
        private System.Type GetScriptTypeForAnimation(string animationType)
        {
            if (string.IsNullOrEmpty(animationType))
                return null;
                
            // 애니메이션 타입에 따른 클래스명 매핑
            string className = animationType switch
            {
                "spawn" => "Game.AnimationSystem.Animator.CharacterAnimation.SpawnAnimation.DefaultCharacterSpawnAnimation",
                "death" => "Game.AnimationSystem.Animator.CharacterAnimation.DeathAnimation.DefaultCharacterDeathAnimation",
                "attack" => "Game.AnimationSystem.Animator.CharacterAnimation.AttackAnimation.DefaultCharacterAttackAnimation",
                _ => animationType // 기본값으로 원본 사용
            };
                
            // 먼저 전체 타입명으로 시도
            var type = System.Type.GetType(className);
            if (type == null)
            {
                // 현재 어셈블리에서 타입 찾기 시도
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    type = assembly.GetType(className);
                    if (type != null)
                        break;
                }
            }
            
            if (type == null)
            {
                Debug.LogError($"[CharacterAnimationSettings] 애니메이션 타입을 찾을 수 없습니다: {animationType} -> {className}");
            }
            return type;
        }
        
        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        /// <param name="scriptType">애니메이션 스크립트 타입</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="useEasing">이징 사용 여부</param>
        public CharacterAnimationSettings(string scriptType, float duration = 1.0f, bool useEasing = true)
        {
            this.animationScriptType = scriptType;
            this.duration = duration;
            this.useEasing = useEasing;
            this.customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
    }
} 