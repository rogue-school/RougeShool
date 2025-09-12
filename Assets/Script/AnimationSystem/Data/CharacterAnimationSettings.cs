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
        
        // 파라미터 제거: 모든 애니메이션은 스크립트(001 또는 지정)로만 동작
        
        /// <summary>
        /// 애니메이션 스크립트 타입
        /// </summary>
        public string AnimationScriptType
        {
            get => animationScriptType;
            set => animationScriptType = value;
        }
        
        public float Duration { get => 0f; set { } }
        public bool UseEasing { get => false; set { } }
        public AnimationCurve CustomCurve { get => AnimationCurve.Linear(0,0,1,1); set { } }
        
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
                Debug.LogError($"[CharacterAnimationSettings] 유효한 애니메이션 스크립트를 찾지 못했습니다. animationType={animationType}");
                onComplete?.Invoke();
            }
        }
        
        private System.Type GetScriptTypeForAnimation(string animationType)
        {
            // 1) 인스펙터 타입 우선
            if (!string.IsNullOrEmpty(animationScriptType))
            {
                var type = System.Type.GetType(animationScriptType);
                if (type == null)
                {
                    var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        type = assembly.GetType(animationScriptType);
                        if (type != null)
                            break;
                    }
                }
                if (type != null) return type;
            }

            // 2) 비어있으면 슬롯별 디폴트(001) 고정 사용
            string @default = animationType switch
            {
                "spawn" => "Game.AnimationSystem.Animator.CharacterAnimation.SpawnAnimation.CharacterSpawnAnimation001",
                "death" => "Game.AnimationSystem.Animator.CharacterAnimation.DeathAnimation.CharacterDeathAnimation001",
                _ => null
            };
            if (!string.IsNullOrEmpty(@default))
            {
                var type = System.Type.GetType(@default);
                if (type == null)
                {
                    var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        type = assembly.GetType(@default);
                        if (type != null) break;
                    }
                }
                return type;
            }

            return null;
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
            // 파라미터 제거
        }
    }
} 